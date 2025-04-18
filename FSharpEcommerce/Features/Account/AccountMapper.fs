namespace FSharpEcommerce.Features.Account

open FSharpEcommerce.Extensions
open FSharpEcommerce.Models
open FSharpEcommerce.Services
open FSharpEcommerce.Repositories
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Security.Claims

type AccountMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group = app.MapGroup("/account").WithTags "Account"

            group.MapPost(
                "/login",
                Func<IAuthService, LoginRequest, Task<IResult>>(fun authService request ->
                    LoginModule.login authService request)
            )
            |> ignore

            group.MapPost(
                "/register",
                Func<IAuthService, RegisterRequest, Task<IResult>>(fun authService request ->
                    RegisterModule.register authService request)
            )
            |> ignore

            group
                .MapGet(
                    "/me",
                    Func<IUserRepository, ClaimsPrincipal, Task<IResult>>(fun userRepository user ->
                        MeModule.me userRepository user)
                )
                .RequireAuthorization()
            |> ignore
