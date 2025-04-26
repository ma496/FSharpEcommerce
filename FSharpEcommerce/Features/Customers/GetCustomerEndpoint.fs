namespace FSharpEcommerce.Features.Customers

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type GetCustomerRequest = { Id: int }

type GetCustomerResponse =
    { Id: int
      Name: string
      Email: string
      Phone: string
      Address: string
      City: string
      State: string
      ZipCode: string
      Country: string
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

module GetCustomerModule =
    let private validateGetCustomerRequest (request: GetCustomerRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private getCustomerHandler (connection: IDbConnection) (request: GetCustomerRequest) : Task<IResult> =
        task {
            let! customer = CustomerData.getCustomerById connection request.Id

            match customer with
            | None -> return ResultUtils.notFound "Customer not found"
            | Some customer ->
                let response: GetCustomerResponse =
                    { Id = customer.Id
                      Name = customer.Name
                      Email = customer.Email
                      Phone = customer.Phone
                      Address = customer.Address
                      City = customer.City
                      State = customer.State
                      ZipCode = customer.ZipCode
                      Country = customer.Country
                      CreatedAt = customer.CreatedAt
                      UpdatedAt = customer.UpdatedAt }

                return ResultUtils.ok response
        }

    let getCustomer (connection: IDbConnection) (request: GetCustomerRequest) : Task<IResult> =
        validateRequest validateGetCustomerRequest request (getCustomerHandler connection)
