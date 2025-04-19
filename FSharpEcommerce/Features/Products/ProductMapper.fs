namespace FSharpEcommerce.Features.Products

open FSharpEcommerce.Extensions
open FSharpEcommerce.Models
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Data

type ProductMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group = app.MapGroup("/products").WithTags "Products"

            // Create product endpoint
            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateProductRequest, Task<IResult>>(fun connection request ->
                        CreateProductModule.createProduct connection request)
                )
                .RequireAuthorization("Admin")
            |> ignore
