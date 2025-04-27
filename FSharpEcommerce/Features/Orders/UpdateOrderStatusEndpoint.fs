namespace FSharpEcommerce.Features.Orders

open System
open FSharpEcommerce.Models
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators
open System.Data
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils
open Dapper

type UpdateOrderStatusRequest = {
    OrderId: int
    Status: string
}

type UpdateOrderStatusResponse = {
    OrderId: int
    Status: string
}

module UpdateOrderStatusModule =
    let private validateUpdateOrderStatusRequest (request: UpdateOrderStatusRequest) =
        validate {
            let! _ = validateField request.OrderId [greaterThan "OrderId" 0]
            let! _ = validateField request.Status [required "Status"]
            return request
        }

    let private updateOrderStatusHandler (connection: IDbConnection) (request: UpdateOrderStatusRequest) : Task<IResult> =
        task {
            let! order =
                connection.QueryFirstOrDefaultAsync<Order>(
                    """
                    SELECT * FROM "Orders" WHERE "Id" = @OrderId
                    """,
                    request)
                    |> Async.AwaitTask

            if isNull (box order) then
                return ResultUtils.notFound "Order not found"
            else
                (*
                    Pending → Processing | Cancelled
                    Processing → Shipped | Cancelled
                    Shipped → Delivered
                    Delivered → (final state)
                    Cancelled → (final state)
                *)
                let updateOrderStatus () =
                    connection.ExecuteAsync(
                        """
                        UPDATE "Orders" SET "Status" = @Status WHERE "Id" = @OrderId
                        """,
                        request)
                        |> Async.AwaitTask

                let oldOrderStatus =
                    match OrderStatusHelpers.fromString order.Status with
                    | Ok status -> status
                    | Error error -> failwith error

                let newOrderStatus =
                    match OrderStatusHelpers.fromString request.Status with
                    | Ok status -> status
                    | Error error -> failwith error

                match oldOrderStatus, newOrderStatus with
                | Pending, (Processing | Cancelled) ->
                    let! _ = updateOrderStatus ()
                    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
                | Processing, (Shipped | Cancelled) ->
                    let! _ = updateOrderStatus ()
                    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
                | Shipped, Delivered ->
                    let! _ = updateOrderStatus ()
                    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
                | (Delivered | Cancelled), _ ->
                    return ResultUtils.badRequest "You cannot change the order status if it is delivered or cancelled"
                | _ ->
                    return ResultUtils.badRequest $"Invalid order status transition: {oldOrderStatus} -> {newOrderStatus}"

        }

    let updateOrderStatus (connection: IDbConnection) (request: UpdateOrderStatusRequest) : Task<IResult> =
        validateRequest validateUpdateOrderStatusRequest request (updateOrderStatusHandler connection)


