namespace FSharpEcommerce.Features.Orders

open FSharpEcommerce.Extensions
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Data

type OrdersMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group =
                app.MapGroup("/orders").WithTags "Orders"

            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateOrderRequest, Task<IResult>>(fun connection request ->
                        CreateOrderEndpoint.createOrder connection request)
                )
                .RequireRole "Admin"
                |> ignore
