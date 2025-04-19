namespace FSharpEcommerce.Features.Account

open System.Threading.Tasks
open System.Data
open FSharpEcommerce.Models
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Data
open BCrypt.Net
open System

type RegisterRequest =
    { Username: string
      Email: string
      Password: string }

type RegisterUserResponse =
    { Id: int
      Email: string
      Username: string }

type RegisterRoleResponse = { Id: int; Name: string }

type RegisterResponse =
    { Token: string
      User: RegisterUserResponse
      Roles: RegisterRoleResponse list }

module RegisterModule =
    let register (connection: IDbConnection) (jwtSettings: JwtSettings) (request: RegisterRequest) : Task<IResult> =
        task {
            try
                let passwordHash = BCrypt.HashPassword request.Password

                let userModel =
                    { Id = 0
                      Email = request.Email
                      PasswordHash = passwordHash
                      Username = request.Username
                      CreatedAt = DateTime.UtcNow }

                let! user = UserData.createUser connection userModel
                let! roles = UserData.getUserRoles connection user.Id
                let! token = JwtUtils.generateToken jwtSettings user roles

                return
                    Results.Created(
                        "/account/me",
                        { Token = token
                          User =
                            { Id = user.Id
                              Email = user.Email
                              Username = user.Username }
                          Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
                    )
            with ex ->
                return Results.BadRequest ex.Message
        }
