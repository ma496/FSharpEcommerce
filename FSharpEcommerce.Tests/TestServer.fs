namespace FSharpEcommerce.Tests

open Microsoft.AspNetCore.Mvc.Testing
open FSharpEcommerce
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

/// Custom WebApplicationFactory for testing
type FSharpEcommerceFactory() =
    inherit WebApplicationFactory<Program.Marker>()

    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder
            .UseEnvironment("Testing")
            .ConfigureServices(fun services ->
                // Configure test services if needed
                let n = 123
                ())
        |> ignore

// Module for handling integration testing
module TestServer =
    let private factory =
        new FSharpEcommerceFactory()

    let private deleteDatabase () =
        let configuration =
            factory.Services.GetRequiredService<IConfiguration>()

        let originalConnectionString =
            configuration.GetConnectionString("DefaultConnection")

        // Extract database name
        let databaseName =
            originalConnectionString.Split(';')
            |> Array.find (fun s -> s.StartsWith "Database=")
            |> fun s -> s.Split('=')[1]

        // Connect to a different database like 'postgres'
        let adminConnectionString =
            originalConnectionString.Replace($"Database={databaseName}", "Database=postgres")

        Npgsql.NpgsqlConnection.ClearAllPools()

        use connection =
            new Npgsql.NpgsqlConnection(adminConnectionString)

        connection.Open()

        // Step 1: Revoke connect permission
        use revokeCmd = connection.CreateCommand()
        revokeCmd.CommandText <- $"REVOKE CONNECT ON DATABASE \"{databaseName}\" FROM public"
        revokeCmd.ExecuteNonQuery() |> ignore

        // Step 2: Terminate connections
        use terminateCmd =
            connection.CreateCommand()

        terminateCmd.CommandText <-
            $"""
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{databaseName}' AND pid <> pg_backend_pid()
        """

        terminateCmd.ExecuteNonQuery() |> ignore

        // Step 3: Drop the database
        use dropCmd = connection.CreateCommand()
        dropCmd.CommandText <- $"DROP DATABASE IF EXISTS \"{databaseName}\""
        dropCmd.ExecuteNonQuery() |> ignore


    // Creates a test client that can be used to send HTTP requests to the application
    let createClient () =
        let client =
            factory.CreateClient(new WebApplicationFactoryClientOptions(AllowAutoRedirect = false))

        client.DefaultRequestHeaders.Add("Accept", "application/json")
        client

    // Creates a test client with authorization header set
    let createAuthenticatedClient (token: string) =
        let client = createClient ()
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}")
        client

    // Dispose once after all tests
    let dispose () =
        deleteDatabase ()
        factory.Dispose()
