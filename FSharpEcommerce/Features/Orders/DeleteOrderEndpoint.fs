namespace FSharpEcommerce.Features.Orders

open System.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators
open Dapper

type DeleteOrderRequest = { Id: int }

module DeleteOrderModule =
    open FSharpEcommerce.Models
    let private validateDeleteOrderRequest (request: DeleteOrderRequest) =
        validate {
            let! _ = greaterThan "Id" 0 request.Id
            return request
        }

    let private deleteOrderHandler (connection: IDbConnection) (request: DeleteOrderRequest) : Task<IResult> =
        task {
            let! order =
                connection.QueryFirstOrDefaultAsync<Order>(
                    """
                    SELECT * FROM "Orders" WHERE "Id" = @Id
                    """,
                    request)
                    |> Async.AwaitTask

            if isNull (box order) then
                return ResultUtils.notFound "Order not found"
            else
                let orderStatus =
                    match OrderStatusHelpers.fromString order.Status with
                    | Ok status -> status
                    | Error error -> failwith error

                match orderStatus with
                | Pending | Cancelled ->
                    let! _ =
                        connection.ExecuteAsync(
                            """
                            DELETE FROM "Orders" WHERE "Id" = @Id
                            """,
                            request)
                            |> Async.AwaitTask
                    return ResultUtils.noContent
                | _ ->
                    return ResultUtils.badRequest "Cannot delete an order with status other than Pending or Cancelled"
        }

    let deleteOrder (connection: IDbConnection) (request: DeleteOrderRequest) : Task<IResult> =
        validateRequest validateDeleteOrderRequest request (deleteOrderHandler connection)