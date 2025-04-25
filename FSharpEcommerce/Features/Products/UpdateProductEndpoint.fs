namespace FSharpEcommerce.Features.Products

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models
open System
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type UpdateProductRequest =
    { Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int }

type UpdateProductResponse =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: DateTime
      UpdatedAt: DateTime option }

module UpdateProductModule =
    let private validateUpdateProductRequest (request: UpdateProductRequest) =
        validate {
            let! _ =
                validateField
                    request.Name
                    [ required "Name"
                      minLength "Name" 2
                      maxLength "Name" 100 ]

            let! _ =
                validateField
                    request.Description
                    [ minLength "Description" 10
                      maxLength "Description" 255 ]

            let! _ = validateField request.Price [ greaterThan "Price" 0m ]

            let! _ = validateField request.StockQuantity [ greaterThanOrEqual "StockQuantity" 0 ]

            let! _ = validateField request.CategoryId [ greaterThan "CategoryId" 0 ]

            return request
        }

    let private updateProductHandler
        (connection: IDbConnection)
        (id: int)
        (request: UpdateProductRequest)
        : Task<IResult> =
        task {
            let! existingProduct = ProductData.getProductById connection id

            match box existingProduct with
            | null -> return ResultUtils.notFound "Product not found"
            | _ ->
                let updatedProduct: Product =
                    { Id = id
                      Name = request.Name
                      Description = request.Description
                      Price = request.Price
                      StockQuantity = request.StockQuantity
                      CategoryId = request.CategoryId
                      CreatedAt = existingProduct.CreatedAt
                      UpdatedAt = Some(DateTime.UtcNow) }

                let! result = ProductData.updateProduct connection updatedProduct

                let response: UpdateProductResponse =
                    { Id = result.Id
                      Name = result.Name
                      Description = result.Description
                      Price = result.Price
                      StockQuantity = result.StockQuantity
                      CategoryId = result.CategoryId
                      CreatedAt = result.CreatedAt
                      UpdatedAt = result.UpdatedAt }

                return ResultUtils.ok response
        }

    let updateProduct (connection: IDbConnection) (id: int) (request: UpdateProductRequest) : Task<IResult> =
        validateRequest validateUpdateProductRequest request (updateProductHandler connection id)
