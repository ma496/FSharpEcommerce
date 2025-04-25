namespace FSharpEcommerce.Tests

open System
open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open FSharpEcommerce
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting
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

/// Module for handling integration testing
module TestServer =
    /// Creates a test client that can be used to send HTTP requests to the application
    let createClient () =
        let factory = new FSharpEcommerceFactory()

        let client =
            factory.CreateClient(new WebApplicationFactoryClientOptions(AllowAutoRedirect = false))

        client.DefaultRequestHeaders.Add("Accept", "application/json")
        factory, client

    /// Creates a test client with authorization header set
    let createAuthenticatedClient (token: string) =
        let factory, client = createClient ()
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}")
        factory, client

    /// Disposes the factory and client
    let dispose (factory: WebApplicationFactory<Program.Marker>, client: HttpClient) =
        client.Dispose()
        factory.Dispose()
