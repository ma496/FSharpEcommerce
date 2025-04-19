namespace FSharpEcommerce.Features.Account

open System.Security.Claims
open System.Threading.Tasks
open System.Data
open FSharpEcommerce.Data
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Utils

type MeRoleResponse = { Id: int; Name: string }

type MeUserResponse =
    { Id: int
      Email: string
      Username: string
      Roles: MeRoleResponse list }

type MeResponse = { User: MeUserResponse }

module MeModule =
    let me (connection: IDbConnection) (user: ClaimsPrincipal) : Task<IResult> =
        task {
            if not user.Identity.IsAuthenticated then
                return ResultUtils.unauthorized "Authentication required"
            else
                let userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)

                if userIdClaim = null then
                    return ResultUtils.unauthorized "Invalid authentication token"
                else
                    let userId = int userIdClaim.Value
                    let! userOption = UserData.getUserById connection userId

                    match userOption with
                    | Some foundUser ->
                        let! roles = UserData.getUserRoles connection foundUser.Id

                        return
                            ResultUtils.ok
                                { User =
                                    { Id = foundUser.Id
                                      Email = foundUser.Email
                                      Username = foundUser.Username
                                      Roles = roles |> List.map (fun role -> { Id = role.Id; Name = role.Name }) } }
                    | None -> return ResultUtils.notFound "User not found"
        }
