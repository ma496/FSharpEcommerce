namespace FSharpEcommerce.Tests.Setup

open System
open Microsoft.Extensions.DependencyInjection
open System.Data
open Npgsql
open Microsoft.Extensions.Configuration

type CustomFixture() =
    // Create test web application with Testing environment
    let webAppFactory =
        new CustomWebApplicationFactory()

    let app = webAppFactory.Server

    // Get connection string for testing database
    let connectionString =
        let services = webAppFactory.Services

        let config =
            services.GetRequiredService<IConfiguration>()

        config.GetConnectionString "DefaultConnection"

    do

        // Initialize test data
        let connection =
            new NpgsqlConnection(connectionString) :> IDbConnection

        TestDataSeeder.seed connection
        |> Async.AwaitTask
        |> Async.RunSynchronously

        connection.Dispose()

    member _.App = app

    member _.GetConnection() =
        new NpgsqlConnection(connectionString) :> IDbConnection

    interface IDisposable with
        member _.Dispose() =
            // Cleanup database
            use connection =
                new NpgsqlConnection(connectionString)

            connection.Open()

            // Drop all tables - this will cascade to related tables
            let dropTables (conn: NpgsqlConnection) =
                use command =
                    new NpgsqlCommand(
                        "DO $$ DECLARE
                        r RECORD;
                    BEGIN
                        FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                            EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.tablename) || ' CASCADE';
                        END LOOP;
                    END $$;",
                        conn
                    )

                command.ExecuteNonQuery() |> ignore

            dropTables connection

            // Dispose application
            app.Dispose()
            webAppFactory.Dispose()

            printfn "Fixture disposed, database cleaned up"
