namespace FSharpEcommerce.Models

open System

/// Represents a product in the database
type Product =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: DateTime
      UpdatedAt: DateTime option }

/// Request model for creating a new product
type CreateProductRequest =
    { Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int }

/// Response model for product operations
type ProductResponse =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: DateTime
      UpdatedAt: DateTime option }
