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

type CreateProductRequest =
    { Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int }

type CreateProductResponse =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: DateTime
      UpdatedAt: DateTime option }

module CreateProductModule =
    let private validateCreateProductRequest (request: CreateProductRequest) =
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

    let private createProductHandler (connection: IDbConnection) (request: CreateProductRequest) : Task<IResult> =
        task {
            let product: Product =
                { Id = 0
                  Name = request.Name
                  Description = request.Description
                  Price = request.Price
                  StockQuantity = request.StockQuantity
                  CategoryId = request.CategoryId
                  CreatedAt = DateTime.UtcNow
                  UpdatedAt = None }

            let! createdProduct = ProductData.createProduct connection product

            let response: CreateProductResponse =
                { Id = createdProduct.Id
                  Name = createdProduct.Name
                  Description = createdProduct.Description
                  Price = createdProduct.Price
                  StockQuantity = createdProduct.StockQuantity
                  CategoryId = createdProduct.CategoryId
                  CreatedAt = createdProduct.CreatedAt
                  UpdatedAt = createdProduct.UpdatedAt }

            return ResultUtils.created response
        }

    let createProduct (connection: IDbConnection) (request: CreateProductRequest) : Task<IResult> =
        validateRequest validateCreateProductRequest request (createProductHandler connection)
