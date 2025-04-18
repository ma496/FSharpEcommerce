namespace FSharpEcommerce.Modules

open FSharpEcommerce.Extensions
open Microsoft.AspNetCore.Builder
open System

module AccountModule =
    let signin () = "Hello World!!!!"

type AccountEndpoints() =
    interface IEndpointMapper with
        member this.Map(app: WebApplication) =
            let group = app.MapGroup("/account")
            group.MapGet("/signin", Func<string>(AccountModule.signin)) |> ignore
