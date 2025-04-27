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
                .MapGet(
                    "/",
                    Func<IDbConnection, Task<IResult>>(fun connection ->
                        GetOrdersModule.getOrders connection)
                )
                .RequireAuthorization()
                |> ignore

            group
                .MapGet(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        GetOrderModule.getOrder connection { Id = id })
                )
                .RequireAuthorization()
                |> ignore

            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateOrderRequest, Task<IResult>>(fun connection request ->
                        CreateOrderModule.createOrder connection request)
                )
                .RequireRole "Admin"
                |> ignore

            group
                .MapPost(
                    "/update-status",
                    Func<IDbConnection, UpdateOrderStatusRequest, Task<IResult>>(fun connection request ->
                        UpdateOrderStatusModule.updateOrderStatus connection request)
                )
                .RequireRole "Admin"
                |> ignore

            group
                .MapDelete(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        DeleteOrderModule.deleteOrder connection { Id = id })
                )
                .RequireRole "Admin"
                |> ignore
