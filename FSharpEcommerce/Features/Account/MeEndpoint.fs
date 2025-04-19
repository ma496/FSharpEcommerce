namespace FSharpEcommerce.Features.Account

open System.Security.Claims
open System.Threading.Tasks
open System.Data
open FSharpEcommerce.Data
open Microsoft.AspNetCore.Http

type MeUserResponse =
    { Id: int
      Email: string
      Username: string }

type MeRoleResponse = { Id: int; Name: string }

type MeResponse =
    { User: MeUserResponse
      Roles: MeRoleResponse list }

module MeModule =
    let me (connection: IDbConnection) (user: ClaimsPrincipal) : Task<IResult> =
        task {
            if not user.Identity.IsAuthenticated then
                return Results.Unauthorized()
            else
                let userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)

                if userIdClaim = null then
                    return Results.Unauthorized()
                else
                    let userId = int userIdClaim.Value
                    let! userOption = UserData.getUserById connection userId

                    match userOption with
                    | Some foundUser ->
                        let! roles = UserData.getUserRoles connection foundUser.Id

                        return
                            Results.Ok
                                { User =
                                    { Id = foundUser.Id
                                      Email = foundUser.Email
                                      Username = foundUser.Username }
                                  Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) }
                    | None -> return Results.NotFound()
        }
