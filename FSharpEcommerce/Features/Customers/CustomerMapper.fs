namespace FSharpEcommerce.Features.Customers

open FSharpEcommerce.Extensions
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Data

type CustomerMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group =
                app.MapGroup("/customers").WithTags "Customers"

            group
                .MapGet(
                    "/",
                    Func<IDbConnection, Task<IResult>>(fun connection -> GetCustomersModule.getCustomers connection)
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapGet(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        GetCustomerModule.getCustomer connection { Id = id })
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateCustomerRequest, Task<IResult>>(fun connection request ->
                        CreateCustomerModule.createCustomer connection request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapPut(
                    "/{id:int}",
                    Func<IDbConnection, int, UpdateCustomerRequest, Task<IResult>>(fun connection id request ->
                        UpdateCustomerModule.updateCustomer connection id request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapDelete(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        DeleteCustomerModule.deleteCustomer connection { Id = id })
                )
                .RequireRole
                "Admin"
            |> ignore
