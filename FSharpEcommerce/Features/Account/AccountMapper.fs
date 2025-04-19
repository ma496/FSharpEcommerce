namespace FSharpEcommerce.Features.Account

open FSharpEcommerce.Extensions
open FSharpEcommerce.Models
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Security.Claims
open System.Data

type AccountMapper() =
    interface IEndpointMapper with
        member _.Map(app: WebApplication) =
            let group = app.MapGroup("/account").WithTags "Account"

            group.MapPost(
                "/login",
                Func<IDbConnection, JwtSettings, LoginRequest, Task<IResult>>(fun connection jwtSettings request ->
                    LoginModule.login connection jwtSettings request)
            )
            |> ignore

            group.MapPost(
                "/register",
                Func<IDbConnection, JwtSettings, RegisterRequest, Task<IResult>>(fun connection jwtSettings request ->
                    RegisterModule.register connection jwtSettings request)
            )
            |> ignore

            group
                .MapGet(
                    "/me",
                    Func<IDbConnection, ClaimsPrincipal, Task<IResult>>(fun connection user ->
                        MeModule.me connection user)
                )
                .RequireAuthorization()
            |> ignore
