namespace FSharpEcommerce.Repositories

open System.Threading.Tasks
open System.Data
open Dapper
open FSharpEcommerce.Models

type IUserRepository =
    abstract member GetUserByEmail: string -> Task<User option>
    abstract member GetUserById: int -> Task<User option>
    abstract member CreateUser: RegisterRequest -> string -> Task<User>
    abstract member GetUserRoles: int -> Task<Role list>

type UserRepository(connection: IDbConnection, roleRepository: IRoleRepository) =
    interface IUserRepository with
        member this.GetUserByEmail(email: string) =
            task {
                let! user =
                    connection.QuerySingleOrDefaultAsync<User>(
                        "SELECT * FROM \"Users\" WHERE \"Email\" = @Email",
                        {| Email = email |}
                    )
                    |> Async.AwaitTask

                return if isNull (box user) then None else Some user
            }

        member this.GetUserById(id: int) =
            task {
                let! user =
                    connection.QuerySingleOrDefaultAsync<User>(
                        "SELECT * FROM \"Users\" WHERE \"Id\" = @Id",
                        {| Id = id |}
                    )
                    |> Async.AwaitTask

                return if isNull (box user) then None else Some user
            }

        member this.CreateUser (request: RegisterRequest) (passwordHash: string) =
            task {
                // Insert user
                let user =
                    { Id = 0
                      Username = request.Username
                      Email = request.Email
                      PasswordHash = passwordHash
                      CreatedAt = System.DateTime.UtcNow }

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
                let! userRoleOption = roleRepository.GetRoleByName("User")

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

        member this.GetUserRoles(userId: int) =
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
