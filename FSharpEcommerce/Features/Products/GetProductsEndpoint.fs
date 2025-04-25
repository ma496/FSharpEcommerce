namespace FSharpEcommerce.Features.Products

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http

type ProductResponse =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

type GetProductsResponse = { Products: ProductResponse list }

module GetProductsModule =
    let getProducts (connection: IDbConnection) : Task<IResult> =
        task {
            let! products = ProductData.getProducts connection

            let productResponses =
                products
                |> Seq.map (fun product ->
                    let response: ProductResponse =
                        { Id = product.Id
                          Name = product.Name
                          Description = product.Description
                          Price = product.Price
                          StockQuantity = product.StockQuantity
                          CategoryId = product.CategoryId
                          CreatedAt = product.CreatedAt
                          UpdatedAt = product.UpdatedAt }

                    response)
                |> Seq.toList

            let response: GetProductsResponse =
                { Products = productResponses }

            return ResultUtils.ok response
        }
