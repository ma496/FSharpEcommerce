namespace FSharpEcommerce.Features.Account

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models
open BCrypt.Net

type LoginRequest = { Email: string; Password: string }

type LoginUserResponse =
    { Id: int
      Email: string
      Username: string }

type LoginRoleResponse = { Id: int; Name: string }

type LoginResponse =
    { Token: string
      User: LoginUserResponse
      Roles: LoginRoleResponse list }

module LoginModule =
    let login (connection: IDbConnection) (jwtSettings: JwtSettings) (request: LoginRequest) : Task<IResult> =
        task {
            let! userOption = UserData.getUserByEmail connection request.Email

            match userOption with
            | None -> return Results.BadRequest "Invalid credentials"
            | Some user ->
                let isPasswordValid = BCrypt.Verify(request.Password, user.PasswordHash)

                if not isPasswordValid then
                    return Results.BadRequest "Invalid credentials"
                else
                    let! roles = UserData.getUserRoles connection user.Id
                    let! token = JwtUtils.generateToken jwtSettings user roles

                    return
                        Results.Ok
                            { Token = token
                              User =
                                { Id = user.Id
                                  Email = user.Email
                                  Username = user.Username }
                              Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
        }
