namespace FSharpEcommerce.Features.Products

open FSharpEcommerce.Extensions
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Data

type ProductMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group =
                app.MapGroup("/products").WithTags "Products"

            group
                .MapGet(
                    "/",
                    Func<IDbConnection, Task<IResult>>(fun connection -> GetProductsModule.getProducts connection)
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapGet(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        GetProductModule.getProduct connection { Id = id })
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateProductRequest, Task<IResult>>(fun connection request ->
                        CreateProductModule.createProduct connection request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapPut(
                    "/{id:int}",
                    Func<IDbConnection, int, UpdateProductRequest, Task<IResult>>(fun connection id request ->
                        UpdateProductModule.updateProduct connection id request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapDelete(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        DeleteProductModule.deleteProduct connection { Id = id })
                )
                .RequireRole
                "Admin"
            |> ignore
