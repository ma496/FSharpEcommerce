module LoginModule

open FSharpEcommerce.Services
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models

let login (authService: IAuthService) (request: LoginRequest) : Task<IResult> =
    task {
        let! response = authService.Login request

        match response with
        | Some authResponse -> return Results.Ok(authResponse)
        | None -> return Results.BadRequest "Invalid credentials"
    }
