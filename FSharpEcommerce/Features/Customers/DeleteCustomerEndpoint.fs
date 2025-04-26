namespace FSharpEcommerce.Features.Customers

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type DeleteCustomerRequest = { Id: int }

module DeleteCustomerModule =
    let private validateDeleteCustomerRequest (request: DeleteCustomerRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private deleteCustomerHandler (connection: IDbConnection) (request: DeleteCustomerRequest) : Task<IResult> =
        task {
            let! customer = CustomerData.getCustomerById connection request.Id

            match customer with
            | None -> return ResultUtils.notFound "Customer not found"
            | Some customer ->
                let! _ = CustomerData.deleteCustomer connection request.Id
                return ResultUtils.noContent
        }

    let deleteCustomer (connection: IDbConnection) (request: DeleteCustomerRequest) : Task<IResult> =
        validateRequest validateDeleteCustomerRequest request (deleteCustomerHandler connection)
