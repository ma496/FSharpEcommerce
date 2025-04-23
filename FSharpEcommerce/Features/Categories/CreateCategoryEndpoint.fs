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

type CreateCategoryRequest = { Name: string; Description: string }

type CreateCategoryResponse =
    { Id: int
      Name: string
      Description: string }

module CreateCategoryModule =
    let private validateCreateCategoryRequest (request: CreateCategoryRequest) =
        validate {
            let! _ = validateField request.Name [ required "Name"; maxLength "Name" 100 ]
            let! _ = validateField request.Description [ minLength "Description" 10; maxLength "Description" 255 ]

            return request
        }

    let private createCategoryHandler (connection: IDbConnection) (request: CreateCategoryRequest) : Task<IResult> =
        task {
            let category: Category =
                { Id = 0
                  Name = request.Name
                  Description = request.Description
                  CreatedAt = DateTime.UtcNow
                  UpdatedAt = None }

            let! createdCategory = CategoryData.createCategory connection category

            let response: CreateCategoryResponse =
                { Id = createdCategory.Id
                  Name = createdCategory.Name
                  Description = createdCategory.Description }

            return ResultUtils.created response
        }

    let createCategory (connection: IDbConnection) (request: CreateCategoryRequest) : Task<IResult> =
        validateRequest validateCreateCategoryRequest request (createCategoryHandler connection)
