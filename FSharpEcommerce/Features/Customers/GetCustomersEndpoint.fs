namespace FSharpEcommerce.Features.Customers

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http

type CustomerResponse =
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

type GetCustomersResponse = { Customers: CustomerResponse list }

module GetCustomersModule =
    let getCustomers (connection: IDbConnection) : Task<IResult> =
        task {
            let! customers = CustomerData.getCustomers connection

            let customerResponses =
                customers
                |> Seq.map (fun customer ->
                    let response: CustomerResponse =
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

                    response)
                |> Seq.toList

            let response: GetCustomersResponse =
                { Customers = customerResponses }

            return ResultUtils.ok response
        }
