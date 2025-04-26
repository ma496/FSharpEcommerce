namespace FSharpEcommerce.Features.Customers

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models
open System
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type UpdateCustomerRequest =
    { Name: string
      Email: string
      Phone: string
      Address: string
      City: string
      State: string
      ZipCode: string
      Country: string }

type UpdateCustomerResponse =
    { Id: int
      Name: string
      Email: string
      Phone: string
      Address: string
      City: string
      State: string
      ZipCode: string
      Country: string
      CreatedAt: DateTime
      UpdatedAt: DateTime option }

module UpdateCustomerModule =
    let private validateUpdateCustomerRequest (request: UpdateCustomerRequest) =
        validate {
            let! _ =
                validateField
                    request.Name
                    [ required "Name"
                      minLength "Name" 2
                      maxLength "Name" 100 ]

            let! _ = validateField request.Email [ email "Email" ]
            return request
        }

    let private updateCustomerHandler
        (connection: IDbConnection)
        (id: int)
        (request: UpdateCustomerRequest)
        : Task<IResult> =
        task {
            let! existingCustomer = CustomerData.getCustomerById connection id

            match existingCustomer with
            | None -> return ResultUtils.notFound "Customer not found"
            | Some existingCustomer ->
                let updatedCustomer: Customer =
                    { Id = id
                      Name = request.Name
                      Email = request.Email
                      Phone = request.Phone
                      Address = request.Address
                      City = request.City
                      State = request.State
                      ZipCode = request.ZipCode
                      Country = request.Country
                      CreatedAt = existingCustomer.CreatedAt
                      UpdatedAt = Some(DateTime.UtcNow) }

                let! updated = CustomerData.updateCustomer connection updatedCustomer

                let response: UpdateCustomerResponse =
                    { Id = updated.Id
                      Name = updated.Name
                      Email = updated.Email
                      Phone = updated.Phone
                      Address = updated.Address
                      City = updated.City
                      State = updated.State
                      ZipCode = updated.ZipCode
                      Country = updated.Country
                      CreatedAt = updated.CreatedAt
                      UpdatedAt = updated.UpdatedAt }

                return ResultUtils.ok response
        }

    let updateCustomer (connection: IDbConnection) (id: int) (request: UpdateCustomerRequest) : Task<IResult> =
        validateRequest validateUpdateCustomerRequest request (updateCustomerHandler connection id)
