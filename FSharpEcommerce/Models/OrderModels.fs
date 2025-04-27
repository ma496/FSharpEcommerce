namespace FSharpEcommerce.Models

open System

[<CLIMutable>]
type Customer = {
    Id: int
    Name: string
    Email: string
    Phone: string
    Address: string
    City: string
    State: string
    ZipCode: string
    Country: string
    CreatedAt: DateTime
    UpdatedAt: DateTime option
}

type OrderStatus =
    | Pending
    | Processing
    | Shipped
    | Delivered
    | Cancelled

[<CLIMutable>]
type Order = {
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
}

[<CLIMutable>]
type OrderItem = {
    Id: int
    OrderId: int
    ProductId: int
    Quantity: int
    Price: decimal
    CreatedAt: DateTime
    UpdatedAt: DateTime option
}


module OrderStatusHelpers =
    let fromString (s: string): Result<OrderStatus, string> =
        match s with
        | "Pending" -> Ok Pending
        | "Processing" -> Ok Processing
        | "Shipped" -> Ok Shipped
        | "Delivered" -> Ok Delivered
        | "Cancelled" -> Ok Cancelled
        | _ -> Error $"Unknown OrderStatus: {s}"

    let toString (status: OrderStatus) =
        status.ToString()
