namespace FSharpEcommerce.Features.Categories

open FSharpEcommerce.Extensions
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Data

type CategoryMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group = app.MapGroup("/categories").WithTags "Categories"

            group
                .MapGet(
                    "/",
                    Func<IDbConnection, Task<IResult>>(fun connection -> GetCategoriesModule.getCategories connection)
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapGet(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        GetCategoryModule.getCategory connection { Id = id })
                )
                .RequireAuthorization()
            |> ignore

            group
                .MapPost(
                    "/",
                    Func<IDbConnection, CreateCategoryRequest, Task<IResult>>(fun connection request ->
                        CreateCategoryModule.createCategory connection request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapPut(
                    "/{id:int}",
                    Func<IDbConnection, int, UpdateCategoryRequest, Task<IResult>>(fun connection id request ->
                        UpdateCategoryModule.updateCategory connection id request)
                )
                .RequireRole
                "Admin"
            |> ignore

            group
                .MapDelete(
                    "/{id:int}",
                    Func<IDbConnection, int, Task<IResult>>(fun connection id ->
                        DeleteCategoryModule.deleteCategory connection { Id = id })
                )
                .RequireRole
                "Admin"
            |> ignore
