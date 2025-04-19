namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module RoleData =
    let getRoleByName (connection: IDbConnection) (name: string) =
        task {
            let! role =
                connection.QuerySingleOrDefaultAsync<Role>(
                    "SELECT * FROM \"Roles\" WHERE \"Name\" = @Name",
                    {| Name = name |}
                )
                |> Async.AwaitTask

            return if isNull (box role) then None else Some role
        }
