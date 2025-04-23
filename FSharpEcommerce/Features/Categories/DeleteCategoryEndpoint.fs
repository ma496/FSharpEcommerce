namespace FSharpEcommerce.Features.Categories

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type DeleteCategoryRequest = { Id: int }

module DeleteCategoryModule =
    let private validateDeleteCategoryRequest (request: DeleteCategoryRequest) =
        validate {
            let! _ = greaterThan "Id" request.Id 0
            return request
        }

    let private deleteCategoryHandler (connection: IDbConnection) (request: DeleteCategoryRequest) : Task<IResult> =
        task {
            let! existingCategory = CategoryData.getCategoryById connection request.Id

            match existingCategory with
            | None -> return ResultUtils.notFound "Category not found"
            | Some _ ->
                try
                    do! CategoryData.deleteCategory connection request.Id
                    return ResultUtils.noContent
                with ex ->
                    return ResultUtils.serverError (sprintf "Error deleting category: %s" ex.Message)
        }

    let deleteCategory (connection: IDbConnection) (request: DeleteCategoryRequest) : Task<IResult> =
        validateRequest validateDeleteCategoryRequest request (deleteCategoryHandler connection)
