namespace FSharpEcommerce.Repositories

open System.Threading.Tasks
open System.Data
open Dapper
open FSharpEcommerce.Models

type IRoleRepository =
    abstract member GetRoleByName: string -> Task<Role option>

type RoleRepository(connection: IDbConnection) =
    interface IRoleRepository with
        member this.GetRoleByName(name: string) =
            task {
                let! role =
                    connection.QuerySingleOrDefaultAsync<Role>(
                        "SELECT * FROM \"Roles\" WHERE \"Name\" = @Name",
                        {| Name = name |}
                    )
                    |> Async.AwaitTask

                return if isNull (box role) then None else Some role
            }
