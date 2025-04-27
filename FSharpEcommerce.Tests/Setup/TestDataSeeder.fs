namespace FSharpEcommerce.Tests.Setup

open System
open System.Data
open FSharpEcommerce.Data
open FSharpEcommerce.Models

module TestDataSeeder =
    let private createCategories (connection: IDbConnection) =
        task {
            let categories =
                CategoryData.getCategories connection
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.toList

            if categories.Length >= 3 then
                return ()
            else
                for i in 1..3 do
                    let name = $"Category {i}"

                    let category: Category =
                        { Id = 0
                          Name = name
                          Description = ""
                          CreatedAt = DateTime.UtcNow
                          UpdatedAt = Some DateTime.UtcNow }

                    let! _ = CategoryData.createCategory connection category
                    ()
        }

    let private createProducts (connection: IDbConnection) =
        task {
            let categories =
                CategoryData.getCategories connection
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> List.ofSeq

            let products =
                ProductData.getProducts connection
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> List.ofSeq

            if products.Length >= 10 then
                return ()
            else
                for i in 1..10 do
                    let name = $"Product {i}"
                    let description = $"Description {i}"
                    let price = decimal (i * 10)
                    let stockQuantity = i * 100
                    // get a random category id
                    let categoryId =
                        categories.[Random().Next(categories.Length)].Id

                    let product: Product =
                        { Id = 0
                          Name = name
                          Description = description
                          Price = price
                          StockQuantity = stockQuantity
                          CategoryId = categoryId
                          CreatedAt = DateTime.UtcNow
                          UpdatedAt = Some DateTime.UtcNow }

                    let! _ = ProductData.createProduct connection product
                    ()
        }

    let private createCustomers (connection: IDbConnection) =
        task {
            let customers =
                CustomerData.getCustomers connection
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> List.ofSeq

            if customers.Length >= 10 then
                return ()
            else
                for i in 1..10 do
                    let name = $"Customer {i}"
                    let email = $"customer{i}@example.com"
                    let phone = $"1234567890"
                    let address = $"1234 Main St, Anytown, USA {i}"
                    let customer: Customer =
                        { Id = 0
                          Name = name
                          Email = email
                          Phone = phone
                          Address = address
                          City = "Anytown"
                          State = "CA"
                          ZipCode = "12345"
                          Country = "USA"
                          CreatedAt = DateTime.UtcNow
                          UpdatedAt = Some DateTime.UtcNow }

                    let! _ = CustomerData.createCustomer connection customer
                    ()
        }

    let seed (connection: IDbConnection) =
        task {
            do! createCategories connection
            do! createProducts connection
            do! createCustomers connection
        }
