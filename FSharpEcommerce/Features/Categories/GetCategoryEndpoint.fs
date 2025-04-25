namespace FSharpEcommerce.Features.Categories

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type GetCategoryRequest = { Id: int }

type GetCategoryResponse =
    { Id: int
      Name: string
      Description: string
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

module GetCategoryModule =
    let private validateGetCategoryRequest (request: GetCategoryRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private getCategoryHandler (connection: IDbConnection) (request: GetCategoryRequest) : Task<IResult> =
        task {
            let! category = CategoryData.getCategoryById connection request.Id

            match category with
            | Some category ->
                let response: GetCategoryResponse =
                    { Id = category.Id
                      Name = category.Name
                      Description = category.Description
                      CreatedAt = category.CreatedAt
                      UpdatedAt = category.UpdatedAt }

                return ResultUtils.ok response
            | None -> return ResultUtils.notFound "Category not found"
        }

    let getCategory (connection: IDbConnection) (request: GetCategoryRequest) : Task<IResult> =
        validateRequest validateGetCategoryRequest request (getCategoryHandler connection)
