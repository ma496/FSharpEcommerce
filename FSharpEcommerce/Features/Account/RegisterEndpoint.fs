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
    let private validateRegisterRequest (request: RegisterRequest) =
        let minUsernameLength = 3
        let minPasswordLength = 6

        // Use our validation helpers to validate each field
        let usernameResult =
            ValidationUtils.Validators.required "username" request.Username
            |> Result.bind (ValidationUtils.Validators.minLength "username" minUsernameLength)

        let emailResult =
            ValidationUtils.Validators.required "email" request.Email
            |> Result.bind (ValidationUtils.Validators.email "email")

        let passwordResult =
            ValidationUtils.Validators.required "password" request.Password
            |> Result.bind (ValidationUtils.Validators.minLength "password" minPasswordLength)

        // Combine the validation results
        match usernameResult, emailResult, passwordResult with
        | Ok username, Ok email, Ok password ->
            Ok
                { Username = username
                  Email = email
                  Password = password }
        | _ ->
            let errors =
                [ match usernameResult with
                  | Error e -> yield! e
                  | _ -> ()

                  match emailResult with
                  | Error e -> yield! e
                  | _ -> ()

                  match passwordResult with
                  | Error e -> yield! e
                  | _ -> () ]

            Error errors

    // The actual register handler after validation
    let private registerHandler
        (connection: IDbConnection)
        (jwtSettings: JwtSettings)
        (request: RegisterRequest)
        : Task<IResult> =
        task {
            try
                // Check if email already exists
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
                                  Username = user.Username }
                              Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
            with ex ->
                return ResultUtils.serverError "An unexpected error occurred while registering"
        }

    // Public register function that first validates the request
    let register (connection: IDbConnection) (jwtSettings: JwtSettings) (request: RegisterRequest) : Task<IResult> =
        ValidationUtils.validateRequest validateRegisterRequest request (registerHandler connection jwtSettings)
