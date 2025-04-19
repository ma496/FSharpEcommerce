namespace FSharpEcommerce.Data

open System.Data
open Dapper
open FSharpEcommerce.Models

module UserData =
    let getUserByEmail (connection: IDbConnection) (email: string) =
        task {
            let! user =
                connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM \"Users\" WHERE \"Email\" = @Email",
                    {| Email = email |}
                )
                |> Async.AwaitTask

            return if isNull (box user) then None else Some user
        }

    let getUserById (connection: IDbConnection) (id: int) =
        task {
            let! user =
                connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM \"Users\" WHERE \"Id\" = @Id", {| Id = id |})
                |> Async.AwaitTask

            return if isNull (box user) then None else Some user
        }

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

    let createUser (connection: IDbConnection) (user: User) =
        task {
            // Insert user
            let sql =
                """
                INSERT INTO "Users" ("Username", "Email", "PasswordHash", "CreatedAt")
                VALUES (@Username, @Email, @PasswordHash, @CreatedAt)
                RETURNING "Id"
            """

            let! userId = connection.ExecuteScalarAsync<int>(sql, user) |> Async.AwaitTask

            // Assign default "User" role
            let userRoleSql =
                """
                INSERT INTO "UserRoles" ("UserId", "RoleId")
                VALUES (@UserId, @RoleId)
            """

            // Get the user role id
            let! userRoleOption = getRoleByName connection "User"

            let roleId =
                match userRoleOption with
                | Some role -> role.Id
                | None -> 1 // Fallback to role id 1 if not found

            do!
                connection.ExecuteAsync(userRoleSql, {| UserId = userId; RoleId = roleId |})
                |> Async.AwaitTask
                |> Async.Ignore
                |> Async.StartAsTask

            return { user with Id = userId }
        }

    let getUserRoles (connection: IDbConnection) (userId: int) =
        task {
            let sql =
                """
                SELECT r.* FROM "Roles" r
                INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
                WHERE ur."UserId" = @UserId
            """

            let! roles = connection.QueryAsync<Role>(sql, {| UserId = userId |}) |> Async.AwaitTask

            return roles |> Seq.toList
        }
