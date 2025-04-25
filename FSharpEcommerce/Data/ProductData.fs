namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module ProductData =
    let getProducts (connection: IDbConnection) =
        task {
            let sql = """SELECT * FROM "Products" """

            let! products =
                connection.QueryAsync<Product>(sql)
                |> Async.AwaitTask

            return products
        }

    let getProductById (connection: IDbConnection) (id: int) =
        task {
            let sql =
                """SELECT * FROM "Products" WHERE "Id" = @Id"""

            let! product =
                connection.QuerySingleOrDefaultAsync<Product>(sql, {| Id = id |})
                |> Async.AwaitTask

            return product
        }

    let createProduct (connection: IDbConnection) (product: Product) =
        task {
            let sql =
                """INSERT INTO "Products" ("Name", "Description", "Price", "StockQuantity", "CategoryId", "CreatedAt", "UpdatedAt")
                VALUES (@Name, @Description, @Price, @StockQuantity, @CategoryId, @CreatedAt, @UpdatedAt)
                RETURNING "Id" """

            let! id =
                connection.ExecuteScalarAsync<int>(sql, product)
                |> Async.AwaitTask

            return { product with Id = id }
        }

    let updateProduct (connection: IDbConnection) (product: Product) =
        task {
            let sql =
                """UPDATE "Products"
                SET "Name" = @Name, "Description" = @Description, "Price" = @Price, "StockQuantity" = @StockQuantity, "CategoryId" = @CategoryId, "UpdatedAt" = @UpdatedAt
                WHERE "Id" = @Id"""

            let! _ =
                connection.ExecuteAsync(sql, product)
                |> Async.AwaitTask

            return product
        }

    let deleteProduct (connection: IDbConnection) (id: int) =
        task {
            let sql =
                """DELETE FROM "Products" WHERE "Id" = @Id"""

            let! _ =
                connection.ExecuteAsync(sql, {| Id = id |})
                |> Async.AwaitTask

            return ()
        }
