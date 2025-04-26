namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module CustomerData =
    let getCustomers (connection: IDbConnection) =
        task {
            let sql = """SELECT * FROM "Customers" """

            let! customers =
                connection.QueryAsync<Customer>(sql)
                |> Async.AwaitTask

            return customers
        }

    let getCustomerById (connection: IDbConnection) (id: int) =
        task {
            let sql = """SELECT * FROM "Customers" WHERE "Id" = @Id"""

            let! customer =
                connection.QuerySingleOrDefaultAsync<Customer>(sql, {| Id = id |})
                |> Async.AwaitTask

            return if isNull (box customer) then None else Some customer
        }

    let createCustomer (connection: IDbConnection) (customer: Customer) =
        task {
            let sql =
                """INSERT INTO "Customers" ("Name", "Email", "Phone", "Address", "City", "State", "ZipCode", "Country", "CreatedAt")
                VALUES (@Name, @Email, @Phone, @Address, @City, @State, @ZipCode, @Country, @CreatedAt)
                RETURNING "Id" """

            let! id =
                connection.ExecuteScalarAsync<int>(sql, customer)
                |> Async.AwaitTask

            return { customer with Id = id }
        }

    let updateCustomer (connection: IDbConnection) (customer: Customer) =
        task {
            let sql =
                """UPDATE "Customers"
                SET "Name" = @Name, "Email" = @Email, "Phone" = @Phone, "Address" = @Address, "City" = @City, "State" = @State, "ZipCode" = @ZipCode, "Country" = @Country, "UpdatedAt" = @UpdatedAt
                WHERE "Id" = @Id"""

            let! _ =
                connection.ExecuteAsync(sql, customer)
                |> Async.AwaitTask

            return customer
        }

    let deleteCustomer (connection: IDbConnection) (id: int) =
        task {
            let sql = """DELETE FROM "Customers" WHERE "Id" = @Id"""

            let! result =
                connection.ExecuteAsync(sql, {| Id = id |})
                |> Async.AwaitTask

            return result
        }
