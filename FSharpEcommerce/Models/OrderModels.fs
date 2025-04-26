namespace FSharpEcommerce.Models

open System

[<CLIMutable>]
type Customer = {
    Id: int
    Name: string
    Email: string option
    Phone: string option
    Address: string option
    City: string option
    State: string option
    ZipCode: string option
    Country: string option
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
    Status: OrderStatus
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
    let fromString (s: string) =
        match s with
        | "Pending" -> Pending
        | "Processing" -> Processing
        | "Shipped" -> Shipped
        | "Delivered" -> Delivered
        | "Cancelled" -> Cancelled
        | _ -> failwithf "Unknown OrderStatus: %s" s

    let toString (status: OrderStatus) =
        status.ToString()
