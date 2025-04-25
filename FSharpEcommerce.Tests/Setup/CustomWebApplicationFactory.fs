namespace FSharpEcommerce.Tests.Setup

open FSharpEcommerce
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing

type CustomWebApplicationFactory() =
    inherit WebApplicationFactory<Program.Marker>()

    override _.ConfigureWebHost builder =
        builder.UseEnvironment "Testing" |> ignore

