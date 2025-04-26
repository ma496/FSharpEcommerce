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

type CreateCustomerRequest =
    { Name: string
      Email: string
      Phone: string
      Address: string
      City: string
      State: string
      ZipCode: string
      Country: string }

type CreateCustomerResponse =
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

module CreateCustomerModule =
    let private validateCreateCustomerRequest (request: CreateCustomerRequest) =
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

    let private createCustomerHandler (connection: IDbConnection) (request: CreateCustomerRequest) : Task<IResult> =
        task {
            let customer: Customer =
                { Id = 0
                  Name = request.Name
                  Email = request.Email
                  Phone = request.Phone
                  Address = request.Address
                  City = request.City
                  State = request.State
                  ZipCode = request.ZipCode
                  Country = request.Country
                  CreatedAt = DateTime.UtcNow
                  UpdatedAt = None }

            let! newCustomer = CustomerData.createCustomer connection customer

            let response: CreateCustomerResponse =
                { Id = newCustomer.Id
                  Name = newCustomer.Name
                  Email = newCustomer.Email
                  Phone = newCustomer.Phone
                  Address = newCustomer.Address
                  City = newCustomer.City
                  State = newCustomer.State
                  ZipCode = newCustomer.ZipCode
                  Country = newCustomer.Country
                  CreatedAt = newCustomer.CreatedAt
                  UpdatedAt = newCustomer.UpdatedAt }

            return ResultUtils.created response
        }

    let createCustomer (connection: IDbConnection) (request: CreateCustomerRequest) : Task<IResult> =
        validateRequest validateCreateCustomerRequest request (createCustomerHandler connection)
