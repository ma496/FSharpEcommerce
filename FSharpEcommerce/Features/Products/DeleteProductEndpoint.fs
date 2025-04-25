namespace FSharpEcommerce.Features.Products

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type DeleteProductRequest = { Id: int }

module DeleteProductModule =
    let private validateDeleteProductRequest (request: DeleteProductRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private deleteProductHandler (connection: IDbConnection) (request: DeleteProductRequest) : Task<IResult> =
        task {
            let! product = ProductData.getProductById connection request.Id

            match box product with
            | null -> return ResultUtils.notFound "Product not found"
            | _ ->
                do! ProductData.deleteProduct connection request.Id
                return ResultUtils.noContent
        }

    let deleteProduct (connection: IDbConnection) (request: DeleteProductRequest) : Task<IResult> =
        validateRequest validateDeleteProductRequest request (deleteProductHandler connection)
