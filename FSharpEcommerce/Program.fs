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
open FSharpEcommerce.Services
open FSharpEcommerce.Repositories
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open Npgsql
open System
open System.Data
open Microsoft.AspNetCore.Authorization
open Microsoft.OpenApi.Models
open System.Collections.Generic

module Program =
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

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
        let jwtSecret = builder.Configuration["Jwt:Secret"]
        let jwtIssuer = builder.Configuration["Jwt:Issuer"]
        let jwtAudience = builder.Configuration["Jwt:Audience"]

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
        builder.Services.AddSingleton<JwtSettings>(jwtSettings) |> ignore
        builder.Services.AddScoped<IJwtService, JwtService>() |> ignore
        builder.Services.AddScoped<IAuthService, AuthService>() |> ignore

        // Register database connection
        builder.Services.AddScoped<IDbConnection>(fun provider ->
            new NpgsqlConnection(connectionString) :> IDbConnection)
        |> ignore

        // Register repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>() |> ignore
        builder.Services.AddScoped<IRoleRepository, RoleRepository>() |> ignore

        // Add Swagger services
        builder.Services.AddEndpointsApiExplorer() |> ignore

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

            let securityRequirement = OpenApiSecurityRequirement()

            let securityScheme =
                OpenApiSecurityScheme(Reference = OpenApiReference(Type = ReferenceType.SecurityScheme, Id = "Bearer"))

            securityRequirement.Add(securityScheme, List<string>())
            options.AddSecurityRequirement(securityRequirement))
        |> ignore

        // Build the app
        let app = builder.Build()

        // Run migrations
        use scope = app.Services.CreateScope()
        let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
        DatabaseUtils.createDatabaseIfNotExists connectionString
        runner.MigrateUp()

        // Configure the HTTP request pipeline

        // Add Swagger middleware
        app.UseSwagger() |> ignore

        app.UseSwaggerUI(fun options ->
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "FSharp Ecommerce API V1")
            options.RoutePrefix <- "swagger")
        |> ignore

        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

        app.MapEndpoints()

        app.Run()

        0 // Exit code
