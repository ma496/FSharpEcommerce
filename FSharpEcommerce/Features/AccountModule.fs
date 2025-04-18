namespace FSharpEcommerce.Features

open FSharpEcommerce.Extensions
open FSharpEcommerce.Models
open FSharpEcommerce.Services
open FSharpEcommerce.Repositories
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Security.Claims

module AccountModule =
    /// Handles user login
    let login (authService: IAuthService) (request: LoginRequest) : Task<IResult> =
        task {
            let! response = authService.Login request

            match response with
            | Some authResponse -> return Results.Ok(authResponse)
            | None -> return Results.BadRequest("Invalid credentials")
        }

    /// Handles user registration
    let register (authService: IAuthService) (request: RegisterRequest) : Task<IResult> =
        task {
            try
                let! response = authService.Register request
                return Results.Created("/account/me", response)
            with ex ->
                return Results.BadRequest(ex.Message)
        }

    /// Returns the current authenticated user information
    let me (userRepository: IUserRepository) (user: ClaimsPrincipal) : Task<IResult> =
        task {
            if not user.Identity.IsAuthenticated then
                return Results.Unauthorized()
            else
                let userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)

                if userIdClaim = null then
                    return Results.Unauthorized()
                else
                    let userId = int userIdClaim.Value
                    let! userOption = userRepository.GetUserById(userId)

                    match userOption with
                    | Some foundUser ->
                        let! roles = userRepository.GetUserRoles(foundUser.Id)

                        return
                            Results.Ok(
                                { User = foundUser
                                  Token = "" // Token is not returned on me endpoint
                                  Roles = roles }
                            )
                    | None -> return Results.NotFound()
        }

type AccountEndpoints() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group = app.MapGroup("/account").WithTags "Account"

            // Public endpoints
            group.MapPost(
                "/login",
                Func<IAuthService, LoginRequest, Task<IResult>>(fun authService request ->
                    AccountModule.login authService request)
            )
            |> ignore

            group.MapPost(
                "/register",
                Func<IAuthService, RegisterRequest, Task<IResult>>(fun authService request ->
                    AccountModule.register authService request)
            )
            |> ignore

            group
                .MapGet(
                    "/me",
                    Func<IUserRepository, ClaimsPrincipal, Task<IResult>>(fun userRepository user ->
                        AccountModule.me userRepository user)
                )
                .RequireAuthorization()
            |> ignore
