namespace FSharpEcommerce

open FSharpEcommerce.Migrations
open FluentMigrator.Runner
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FSharpEcommerce.Extensions
open FSharpEcommerce.Utils
open FSharpEcommerce.Models
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open Npgsql
open System
open System.Data
open Microsoft.OpenApi.Models
open System.Collections.Generic
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Diagnostics
open System.Text.Json
open Microsoft.AspNetCore.Hosting

module Program =
    // Marker class for WebApplicationFactory
    type Marker = class end

    // Extract configuration to a separate function
    let configureServices (builder: WebApplicationBuilder) =
        // Add services to the container
        let connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")

        // Configure migrations
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb
                    .AddPostgres11_0()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof<CreateUsersTable>.Assembly)
                    .For.Migrations()
                |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
        |> ignore

        // Configure JWT Authentication
        let jwtSecret =
            builder.Configuration["Jwt:Secret"]

        let jwtIssuer =
            builder.Configuration["Jwt:Issuer"]

        let jwtAudience =
            builder.Configuration["Jwt:Audience"]

        let jwtExpiryMinutes =
            match builder.Configuration["Jwt:ExpiryMinutes"] with
            | null -> 60
            | value -> int value

        let jwtSettings =
            JwtSettings.create jwtSecret jwtIssuer jwtAudience jwtExpiryMinutes

        // Register JWT Authentication
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options ->
                options.TokenValidationParameters <-
                    TokenValidationParameters(
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ClockSkew = TimeSpan.Zero
                    ))
        |> ignore

        // Configure authorization policies
        builder.Services.AddAuthorization(fun options ->
            // Add a policy for "Admin" role
            options.AddPolicy("Admin", fun policy -> policy.RequireRole("Admin") |> ignore)

            // Add a policy for "User" role
            options.AddPolicy("User", fun policy -> policy.RequireRole("User") |> ignore))
        |> ignore

        // Register our services
        builder.Services.AddSingleton<JwtSettings>(jwtSettings)
        |> ignore

        // Register database connection
        builder.Services.AddScoped<IDbConnection>(fun provider ->
            new NpgsqlConnection(connectionString) :> IDbConnection)
        |> ignore

        // Add Swagger services
        builder.Services.AddEndpointsApiExplorer()
        |> ignore

        builder.Services.AddSwaggerGen(fun options ->
            options.SwaggerDoc(
                "v1",
                OpenApiInfo(
                    Title = "FSharp Ecommerce API",
                    Version = "v1",
                    Description = "API for FSharp Ecommerce application",
                    Contact = OpenApiContact(Name = "Support", Email = "support@fsharpecommerce.com")
                )
            )

            // Configure JWT Authentication in Swagger
            options.AddSecurityDefinition(
                "Bearer",
                OpenApiSecurityScheme(
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                )
            )

            let securityRequirement =
                OpenApiSecurityRequirement()

            let securityScheme =
                OpenApiSecurityScheme(Reference = OpenApiReference(Type = ReferenceType.SecurityScheme, Id = "Bearer"))

            securityRequirement.Add(securityScheme, List<string>())
            options.AddSecurityRequirement(securityRequirement))
        |> ignore

        builder

    // Function to configure the application
    let configureApp (app: WebApplication) =
        Dapper.FSharp.PostgreSQL.OptionTypes.register ()

        // Run migrations
        use scope = app.Services.CreateScope()

        let runner =
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

        let connectionString =
            app.Configuration.GetConnectionString("DefaultConnection")

        DatabaseUtils.createDatabaseIfNotExists connectionString
        runner.MigrateUp()

        // Get logger service for error handling
        let logger =
            app.Services.GetRequiredService<ILogger<_>>()

        // Add global error handling middleware (must be added before other middleware)
        app.UseExceptionHandler("/error") |> ignore

        // Add specific error handling endpoint
        app.Map(
            "/error",
            fun (errorApp: IApplicationBuilder) ->
                errorApp.Run(fun (context: HttpContext) ->
                    let exceptionFeature =
                        context.Features.Get<IExceptionHandlerFeature>()

                    if exceptionFeature <> null then
                        let error = exceptionFeature.Error
                        context.Response.StatusCode <- 500
                        context.Response.ContentType <- "application/json"

                        let errorResponse =
                            ApiErrorResponse.serverError error.Message

                        let jsonOptions = JsonSerializerOptions()
                        jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase

                        let jsonResponse =
                            JsonSerializer.Serialize(errorResponse, jsonOptions)

                        context.Response.WriteAsync(jsonResponse)
                    else
                        context.Response.StatusCode <- 500
                        context.Response.ContentType <- "application/json"

                        let errorResponse =
                            ApiErrorResponse.serverError "An unknown error occurred"

                        let jsonOptions = JsonSerializerOptions()
                        jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase

                        let jsonResponse =
                            JsonSerializer.Serialize(errorResponse, jsonOptions)

                        context.Response.WriteAsync(jsonResponse))
        )
        |> ignore

        // Add Swagger middleware
        app.UseSwagger() |> ignore

        app.UseSwaggerUI(fun options ->
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "FSharp Ecommerce API V1")
            options.RoutePrefix <- "swagger")
        |> ignore

        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

        app.MapEndpoints()

        app

    // Public function to create and configure a test application
    let createWebApplication (args: string[]) =
        let builder =
            WebApplication.CreateBuilder(args)

        let builder = configureServices builder
        let app = builder.Build()
        configureApp app

    [<EntryPoint>]
    let main args =
        let app = createWebApplication args
        app.Run()
        0 // Exit code
