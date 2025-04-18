open FSharpEcommerce.Migrations
open FluentMigrator.Runner
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FSharpEcommerce.Extensions
open FSharpEcommerce.Utils

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    let connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")

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

    let app = builder.Build()

    use scope = app.Services.CreateScope()
    // run the migrations
    let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
    DatabaseUtils.createDatabaseIfNotExists connectionString
    runner.MigrateUp()

    app.MapEndpoints()

    app.Run()

    0 // Exit code
