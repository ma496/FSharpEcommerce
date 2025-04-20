namespace FSharpEcommerce.Models

open System

type Product =
    { Id: int
      Name: string
      Description: string
      Price: decimal
      StockQuantity: int
      CategoryId: int
      CreatedAt: DateTime
      UpdatedAt: DateTime option }

type Category =
    { Id: int
      Name: string
      Description: string
      CreatedAt: DateTime
      UpdatedAt: DateTime option }
