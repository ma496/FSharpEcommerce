namespace FSharpEcommerce.Tests.Setup

open Xunit
open System.Net.Http.Headers
open System.Text.Json
open System.Net.Http
open System.Net.Http.Json
open FSharpEcommerce.Features.Account

[<Collection("CustomCollectionFixture")>]
type TestBase(fixture: CustomFixture) =
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

    member this.Login(email: string, password: string) =
        let client = this.CreateClient()

        let response =
            client.PostAsJsonAsync("/account/login", {| Email = email; Password = password |}).Result

        this.ToType<LoginResponse> response

    member this.AdminLogin() =
        this.Login("admin@example.com", "Admin123!")

    member this.UserLogin() =
        this.Login("user@example.com", "User123!")

    interface IClassFixture<CustomFixture>
