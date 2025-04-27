namespace FSharpEcommerce.Features.Orders

open System.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators
open Dapper

type GetOrderRequest = { Id: int }

[<CLIMutable>]
type OrderItemResponse = {
    Id: int
    OrderId: int
    ProductId: int
    Quantity: int
    Price: decimal
}

[<CLIMutable>]
type GetOrderResponse =
    { Id: int
      CustomerId: int
      OrderDate: System.DateTime
      TotalAmount: decimal
      Status: string
      PaymentMethod: string
      ShippingAddress: string
      BillingAddress: string
      CreatedAt: System.DateTime
      UpdatedAt: System.DateTime option
      Items: OrderItemResponse list }

module GetOrderModule =
    let private validateGetOrderRequest (request: GetOrderRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private getOrderHandler (connection: IDbConnection) (request: GetOrderRequest) : Task<IResult> =
        task {
            let! order =
                connection.QuerySingleOrDefaultAsync<GetOrderResponse>(
                    """
                    SELECT * FROM "Orders" WHERE "Id" = @Id
                    """,
                    {| Id = request.Id |}
                )
                |> Async.AwaitTask

            if isNull (box order) then
                return ResultUtils.notFound "Order not found"
            else
                let! orderItems =
                    connection.QueryAsync<OrderItemResponse>(
                        """
                        SELECT * FROM "OrderItems" WHERE "OrderId" = @OrderId
                        """,
                        {| OrderId = request.Id |}
                    )
                    |> Async.AwaitTask

                return ResultUtils.ok { order with Items = List.ofSeq orderItems }
        }

    let getOrder (connection: IDbConnection) (request: GetOrderRequest) : Task<IResult> =
        validateRequest validateGetOrderRequest request (getOrderHandler connection)
