namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module CategoryData =
    let getCategories (connection: IDbConnection) =
        task {
            let sql = """SELECT * FROM "Categories" """
            let! categories = connection.QueryAsync<Category>(sql) |> Async.AwaitTask

            return categories
        }

    let getCategoryById (connection: IDbConnection) (id: int) =
        task {
            let sql = """SELECT * FROM "Categories" WHERE "Id" = @Id"""

            let! category =
                connection.QuerySingleOrDefaultAsync<Category>(sql, {| Id = id |})
                |> Async.AwaitTask

            return if isNull (box category) then None else Some category
        }

    let createCategory (connection: IDbConnection) (category: Category) =
        task {
            let sql =
                """INSERT INTO "Categories" ("Name", "Description", "CreatedAt", "UpdatedAt")
                VALUES (@Name, @Description, @CreatedAt, @UpdatedAt)
                RETURNING "Id" """

            let! id = connection.ExecuteScalarAsync<int>(sql, category) |> Async.AwaitTask

            return { category with Id = id }
        }

    let updateCategory (connection: IDbConnection) (category: Category) =
        task {
            let sql =
                """UPDATE "Categories" 
                SET "Name" = @Name, "Description" = @Description, "UpdatedAt" = @UpdatedAt 
                WHERE "Id" = @Id"""

            let! _ = connection.ExecuteAsync(sql, category) |> Async.AwaitTask

            return category
        }

    let deleteCategory (connection: IDbConnection) (id: int) =
        task {
            let sql = """DELETE FROM "Categories" WHERE "Id" = @Id"""
            let! _ = connection.ExecuteAsync(sql, {| Id = id |}) |> Async.AwaitTask

            return ()
        }
