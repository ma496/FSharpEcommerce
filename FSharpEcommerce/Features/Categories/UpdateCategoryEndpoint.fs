namespace FSharpEcommerce.Features.Categories

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models
open System
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type UpdateCategoryRequest = { Name: string; Description: string }

type UpdateCategoryResponse =
    { Id: int
      Name: string
      Description: string }

module UpdateCategoryModule =
    let private validateUpdateCategoryRequest (request: UpdateCategoryRequest) =
        validate {
            let! _ = validateField request.Name [ required "Name"; minLength "Name" 2; maxLength "Name" 100 ]
            let! _ = validateField request.Description [ minLength "Description" 10; maxLength "Description" 255 ]
            return request
        }

    let private updateCategoryHandler
        (connection: IDbConnection)
        (id: int)
        (request: UpdateCategoryRequest)
        : Task<IResult> =
        task {
            // Check if category exists
            let! existingCategory = CategoryData.getCategoryById connection id

            match existingCategory with
            | None -> return ResultUtils.notFound "Category not found"
            | Some existingCategory ->
                // Update the category
                let updatedCategory =
                    { existingCategory with
                        Name = request.Name
                        Description = request.Description
                        UpdatedAt = Some DateTime.UtcNow }

                let! updated = CategoryData.updateCategory connection updatedCategory

                let response: UpdateCategoryResponse =
                    { Id = updated.Id
                      Name = updated.Name
                      Description = updated.Description }

                return ResultUtils.ok response
        }

    let updateCategory (connection: IDbConnection) (id: int) (request: UpdateCategoryRequest) : Task<IResult> =
        validateRequest validateUpdateCategoryRequest request (updateCategoryHandler connection id)
