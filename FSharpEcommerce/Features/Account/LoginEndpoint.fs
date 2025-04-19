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
    // Define validators for the login request using our custom validation
    let private validateLoginRequest (request: LoginRequest) =
        // Use our validation helpers to validate each field
        let emailResult =
            ValidationUtils.Validators.required "email" request.Email
            |> Result.bind (ValidationUtils.Validators.email "email")

        let passwordResult = ValidationUtils.Validators.required "password" request.Password

        // Combine the validation results
        match emailResult, passwordResult with
        | Ok email, Ok password -> Ok { Email = email; Password = password }
        | _ ->
            let errors =
                [ match emailResult with
                  | Error e -> yield! e
                  | _ -> ()

                  match passwordResult with
                  | Error e -> yield! e
                  | _ -> () ]

            Error errors

    // The actual login handler after validation
    let private loginHandler
        (connection: IDbConnection)
        (jwtSettings: JwtSettings)
        (request: LoginRequest)
        : Task<IResult> =
        task {
            let! userOption = UserData.getUserByEmail connection request.Email

            match userOption with
            | None -> return ResultUtils.unauthorized "Invalid email or password"
            | Some user ->
                let isPasswordValid = BCrypt.Verify(request.Password, user.PasswordHash)

                if not isPasswordValid then
                    return ResultUtils.unauthorized "Invalid email or password"
                else
                    let! roles = UserData.getUserRoles connection user.Id
                    let! token = JwtUtils.generateToken jwtSettings user roles

                    return
                        ResultUtils.ok
                            { Token = token
                              User =
                                { Id = user.Id
                                  Email = user.Email
                                  Username = user.Username }
                              Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
        }

    // Public login function that first validates the request
    let login (connection: IDbConnection) (jwtSettings: JwtSettings) (request: LoginRequest) : Task<IResult> =
        ValidationUtils.validateRequest validateLoginRequest request (loginHandler connection jwtSettings)
