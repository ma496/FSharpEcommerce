namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module CategoryData =
    let getCategories (connection: IDbConnection) =
        task {
            let! categories =
                connection.QueryAsync<Category>("SELECT * FROM public.\"Categories\"")
                |> Async.AwaitTask

            return categories
        }

    let getCategoryById (connection: IDbConnection) (id: int) =
        task {
            let! category =
                connection.QuerySingleOrDefaultAsync<Category>(
                    "SELECT * FROM public.\"Categories\" WHERE \"Id\" = @Id",
                    {| Id = id |}
                )
                |> Async.AwaitTask

            return if isNull (box category) then None else Some category
        }

    let createCategory (connection: IDbConnection) (category: Category) =
        task {
            let sql =
                """INSERT INTO public."Categories" ("Name", "Description", "CreatedAt", "UpdatedAt")
                VALUES (@Name, @Description, @CreatedAt, @UpdatedAt)
                RETURNING "Id" """

            let! id = connection.ExecuteScalarAsync<int>(sql, category) |> Async.AwaitTask

            return { category with Id = id }
        }

    let updateCategory (connection: IDbConnection) (category: Category) =
        task {
            let sql =
                """UPDATE public."Categories" 
                SET "Name" = @Name, "Description" = @Description, "UpdatedAt" = @UpdatedAt 
                WHERE "Id" = @Id"""

            let! _ = connection.ExecuteAsync(sql, category) |> Async.AwaitTask

            return category
        }

    let deleteCategory (connection: IDbConnection) (id: int) =
        task {
            let! _ =
                connection.ExecuteAsync("DELETE FROM public.\"Categories\" WHERE \"Id\" = @Id", {| Id = id |})
                |> Async.AwaitTask

            return ()
        }
