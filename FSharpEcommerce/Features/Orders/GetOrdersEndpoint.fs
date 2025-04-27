namespace FSharpEcommerce.Features.Orders

open System.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open Dapper

[<CLIMutable>]
type OrderResponse =
    { Id: int
      CustomerId: int
      OrderDate: System.DateTime
      TotalAmount: decimal
      Status: string
      PaymentMethod: string
      ShippingAddress: string
      BillingAddress: string
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option }

type GetOrdersResponse = { Orders: OrderResponse list }

module GetOrdersModule =
    let getOrders (connection: IDbConnection) : Task<IResult> =
        task {
            let! orders =
                connection.QueryAsync<OrderResponse>(
                    """
                    SELECT * FROM "Orders"
                    """,
                    null
                )
                |> Async.AwaitTask



            let response: GetOrdersResponse =
                { Orders = List.ofSeq orders }

            return ResultUtils.ok response
        }
