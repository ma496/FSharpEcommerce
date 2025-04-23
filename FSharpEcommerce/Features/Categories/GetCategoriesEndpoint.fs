namespace FSharpEcommerce.Features.Categories

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http

type CategoryResponse =
    { Id: int
      Name: string
      Description: string
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

type GetCategoriesResponse = { Categories: CategoryResponse list }

module GetCategoriesModule =
    let getCategories (connection: IDbConnection) : Task<IResult> =
        task {
            let! categories = CategoryData.getCategories connection

            let categoryResponses =
                categories
                |> Seq.map (fun category ->
                    let response: CategoryResponse =
                        { Id = category.Id
                          Name = category.Name
                          Description = category.Description
                          CreatedAt = category.CreatedAt
                          UpdatedAt = category.UpdatedAt }

                    response)
                |> Seq.toList

            let response: GetCategoriesResponse = { Categories = categoryResponses }

            return ResultUtils.ok response
        }
