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
                // Check if email already exists
                let! existingUser = UserData.getUserByEmail connection request.Email

                match existingUser with
                | Some _ -> return ResultUtils.conflict "A user with this email already exists"
                | None ->
                    // Validate password
                    if String.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 then
                        return
                            ResultUtils.validationError
                                "Password is invalid"
                                (Map [ "password", "Password must be at least 6 characters" ])
                    else
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
                            ResultUtils.created
                                "/account/me"
                                { Token = token
                                  User =
                                    { Id = user.Id
                                      Email = user.Email
                                      Username = user.Username }
                                  Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
            with ex ->
                return ResultUtils.serverError "An unexpected error occurred while registering"
        }
