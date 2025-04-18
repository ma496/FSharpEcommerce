module RegisterModule

open System.Threading.Tasks
open FSharpEcommerce.Models
open FSharpEcommerce.Services
open Microsoft.AspNetCore.Http

let register (authService: IAuthService) (request: RegisterRequest) : Task<IResult> =
    task {
        try
            let! response = authService.Register request
            return Results.Created("/account/me", response)
        with ex ->
            return Results.BadRequest(ex.Message)
    }

