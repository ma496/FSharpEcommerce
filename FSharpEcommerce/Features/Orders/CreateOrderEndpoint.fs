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


type CreateOrderItemRequest = {
    ProductId: int
    Quantity: int
    Price: decimal
}

type CreateOrderRequest = {
    CustomerId: int
    OrderDate: DateTime
    TotalAmount: decimal
    PaymentMethod: string
    ShippingAddress: string
    BillingAddress: string
    Items: CreateOrderItemRequest list
}

[<CLIMutable>]
type CreateOrderItemResponse = {
    Id: int
    ProductId: int
    Quantity: int
    Price: decimal
    CreatedAt: DateTime
    UpdatedAt: DateTime option
}

type CreateOrderResponse = {
    Id: int
    CustomerId: int
    OrderDate: DateTime
    TotalAmount: decimal
    Status: string
    PaymentMethod: string
    ShippingAddress: string
    BillingAddress: string
    CreatedAt: DateTime
    UpdatedAt: DateTime option
    Items: CreateOrderItemResponse list
}

module CreateOrderEndpoint =
    let private validateCreateOrderItemRequest (item: CreateOrderItemRequest) =
        validate {
            let! _ = validateField item.ProductId [greaterThan "ProductId" 0]
            let! _ = validateField item.Quantity [greaterThan "Quantity" 0]
            let! _ = validateField item.Price [greaterThan "Price" 0.0M]
            return item
        }

    let private validateCreateOrderRequest (request: CreateOrderRequest) =
        validate {
            let! _ = validateField request.CustomerId [greaterThan "CustomerId" 0]
            let! _ = validateField request.OrderDate [greaterThan "OrderDate" DateTime.MinValue]
            let! _ = validateField request.TotalAmount [greaterThan "TotalAmount" 0.0M]
            let! _ = validateField request.PaymentMethod [required "PaymentMethod"]
            let! _ = validateField request.ShippingAddress [required "ShippingAddress"]
            let! _ = validateField request.BillingAddress [required "BillingAddress"]

            let! _ = validateList "Items" validateCreateOrderItemRequest request.Items

            return request
        }

    let private createOrderHandler (connection: IDbConnection) (request: CreateOrderRequest) : Task<IResult> =
        task {
            let order: Order =
                { Id = 0
                  CustomerId = request.CustomerId
                  OrderDate = request.OrderDate
                  TotalAmount = request.TotalAmount
                  PaymentMethod = request.PaymentMethod
                  ShippingAddress = request.ShippingAddress
                  BillingAddress = request.BillingAddress
                  Status = OrderStatus.Pending
                  CreatedAt = DateTime.UtcNow
                  UpdatedAt = None }

            // create transaction
            connection.Open()
            use transaction = connection.BeginTransaction()

            // create order
            let createOrderSql = """
            INSERT INTO "Orders" ("CustomerId", "OrderDate", "TotalAmount", "PaymentMethod", "ShippingAddress", "BillingAddress", "Status", "CreatedAt", "UpdatedAt")
            VALUES (@CustomerId, @OrderDate, @TotalAmount, @PaymentMethod, @ShippingAddress, @BillingAddress, @Status, @CreatedAt, @UpdatedAt)
            RETURNING "Id"
            """
            let! orderId =
                connection.ExecuteScalarAsync<int>(createOrderSql, {|order with Status = OrderStatusHelpers.toString order.Status|}, transaction)
                |> Async.AwaitTask

            // create order items
            let createOrderItemsSql = """
            INSERT INTO "OrderItems" ("OrderId", "ProductId", "Quantity", "Price", "CreatedAt", "UpdatedAt")
            VALUES (@OrderId, @ProductId, @Quantity, @Price, @CreatedAt, @UpdatedAt)
            RETURNING "Id"
            """
            let orderItems =
                request.Items
                |> List.map (fun item ->
                    { Id = 0
                      OrderId = orderId
                      ProductId = item.ProductId
                      Quantity = item.Quantity
                      Price = item.Price
                      CreatedAt = DateTime.UtcNow
                      UpdatedAt = None })

            let! _ =
                connection.ExecuteAsync(createOrderItemsSql, orderItems, transaction)
                |> Async.AwaitTask

            // commit transaction
            transaction.Commit()

            let newOrder = { order with Id = orderId }
            let! newOrderItems =
                connection.QueryAsync<CreateOrderItemResponse>(
                    "SELECT * FROM \"OrderItems\" WHERE \"OrderId\" = @OrderId",
                    {| OrderId = orderId |},
                    transaction
                )
                |> Async.AwaitTask

            let response: CreateOrderResponse =
                { Id = newOrder.Id
                  CustomerId = newOrder.CustomerId
                  OrderDate = newOrder.OrderDate
                  TotalAmount = newOrder.TotalAmount
                  Status = OrderStatusHelpers.toString newOrder.Status
                  PaymentMethod = newOrder.PaymentMethod
                  ShippingAddress = newOrder.ShippingAddress
                  BillingAddress = newOrder.BillingAddress
                  CreatedAt = newOrder.CreatedAt
                  UpdatedAt = newOrder.UpdatedAt
                  Items = List.ofSeq newOrderItems }

            return ResultUtils.created response
        }

    let createOrder (connection: IDbConnection) (request: CreateOrderRequest) : Task<IResult> =
        validateRequest validateCreateOrderRequest request (createOrderHandler connection)
