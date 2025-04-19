namespace FSharpEcommerce.Features.Account

open System.Threading.Tasks
open System.Data
open FSharpEcommerce.Models
open FSharpEcommerce.Utils
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Data
open BCrypt.Net
open System
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type RegisterRequest =
    { Username: string
      Email: string
      Password: string }

type RegisterRoleResponse = { Id: int; Name: string }

type RegisterUserResponse =
    { Id: int
      Email: string
      Username: string
      Roles: RegisterRoleResponse list }


type RegisterResponse =
    { Token: string
      User: RegisterUserResponse }

module RegisterModule =
    let private validateRegisterRequest (request: RegisterRequest) =
        validate {
            let! _ = validateField request.Username [ required "Username" ]
            let! _ = validateField request.Email [ required "Email"; email "Email" ]
            let! _ = validateField request.Password [ required "Password"; minLength "Password" 8 ]
            return request
        }

    let private registerHandler
        (connection: IDbConnection)
        (jwtSettings: JwtSettings)
        (request: RegisterRequest)
        : Task<IResult> =
        task {
            let! existingUser = UserData.getUserByEmail connection request.Email

            match existingUser with
            | Some _ -> return ResultUtils.conflict "A user with this email already exists"
            | None ->
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
                              Username = user.Username
                              Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) } }
        }

    let register (connection: IDbConnection) (jwtSettings: JwtSettings) (request: RegisterRequest) : Task<IResult> =
        validateRequest validateRegisterRequest request (registerHandler connection jwtSettings)
