namespace FSharpEcommerce.Tests.Setup

open Xunit
open System.Net.Http.Headers
open System.Text.Json
open System.Net.Http

[<Collection("CollectionFixture")>]
type TestBase(fixture: Fixture) =
    member _.CreateClient() =
        let client = fixture.App.CreateClient()
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue "application/json")
        client

    member this.CreateAuthenticatedClient(token: string) =
        let client = this.CreateClient()
        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
        client

    member _.ToType<'T>(content: string) =
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        JsonSerializer.Deserialize<'T>(content, options)

    member this.ToType<'T>(response: HttpResponseMessage) =
        let content =
            response.Content.ReadAsStringAsync().Result

        this.ToType<'T>(content)

    interface IClassFixture<Fixture>
