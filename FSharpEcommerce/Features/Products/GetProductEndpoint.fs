namespace FSharpEcommerce.Features.Products

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type GetProductRequest = { Id: int }

type GetProductResponse =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

module GetProductModule =
    let private validateGetProductRequest (request: GetProductRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private getProductHandler (connection: IDbConnection) (request: GetProductRequest) : Task<IResult> =
        task {
            let! product = ProductData.getProductById connection request.Id

            match box product with
            | null -> return ResultUtils.notFound "Product not found"
            | _ ->
                let response: GetProductResponse =
                    { Id = product.Id
                      Name = product.Name
                      Description = product.Description
                      Price = product.Price
                      StockQuantity = product.StockQuantity
                      CategoryId = product.CategoryId
                      CreatedAt = product.CreatedAt
                      UpdatedAt = product.UpdatedAt }

                return ResultUtils.ok response
        }

    let getProduct (connection: IDbConnection) (request: GetProductRequest) : Task<IResult> =
        validateRequest validateGetProductRequest request (getProductHandler connection)
