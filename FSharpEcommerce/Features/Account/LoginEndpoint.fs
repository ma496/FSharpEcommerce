namespace FSharpEcommerce.Features.Account

open System.Data
open FSharpEcommerce.Data
open System.Threading.Tasks
open FSharpEcommerce.Utils
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models
open BCrypt.Net

type LoginRequest = { Email: string; Password: string }

type LoginRoleResponse = { Id: int; Name: string }

type LoginUserResponse =
    { Id: int
      Email: string
      Username: string
      Roles: LoginRoleResponse list }

type LoginResponse =
    { Token: string
      User: LoginUserResponse }

module LoginModule =
    let private validateLoginRequest (request: LoginRequest) =
        validate {
            let! _ = validateField request.Email [ required "Email"; email "Email" ]

            let! _ = validateField request.Password [ required "Password"; minLength "Password" 8 ]

            return request
        }

    let private loginHandler
        (connection: IDbConnection)
        (jwtSettings: JwtSettings)
        (request: LoginRequest)
        : Task<IResult> =
        task {
            let! userOption = UserData.getUserByEmail connection request.Email

            match userOption with
            | None -> return ResultUtils.badRequest "Invalid email or password"
            | Some user ->
                let isPasswordValid = BCrypt.Verify(request.Password, user.PasswordHash)

                if not isPasswordValid then
                    return ResultUtils.badRequest "Invalid email or password"
                else
                    let! roles = UserData.getUserRoles connection user.Id
                    let! token = JwtUtils.generateToken jwtSettings user roles

                    return
                        ResultUtils.ok
                            { Token = token
                              User =
                                { Id = user.Id
                                  Email = user.Email
                                  Username = user.Username
                                  Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) } }
        }

    let login (connection: IDbConnection) (jwtSettings: JwtSettings) (request: LoginRequest) : Task<IResult> =
        validateRequest validateLoginRequest request (loginHandler connection jwtSettings)
