namespace FSharpEcommerce.Utils

open Npgsql

module DatabaseUtils =
    let createDatabaseIfNotExists (connectionString: string) =
        let builder = NpgsqlConnectionStringBuilder(connectionString)
        let databaseName = builder.Database

        // Save the database name and then change to a default database to check if our target exists
        builder.Database <- "postgres"

        use connection = new NpgsqlConnection(builder.ConnectionString)
        connection.Open()

        // Check if the database exists
        use command =
            new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", connection)

        let exists = command.ExecuteScalar() <> null

        if not exists then
            // Create the database
            use createCommand =
                new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection)

            createCommand.ExecuteNonQuery() |> ignore
            printfn $"Database %s{databaseName} created successfully."
        else
            printfn $"Database %s{databaseName} already exists."
