module MeModule

open System.Security.Claims
open System.Threading.Tasks
open FSharpEcommerce.Repositories
open Microsoft.AspNetCore.Http

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
                            {| User = foundUser
                               Token = "" // Token is not returned on me endpoint
                               Roles = roles |}
                        )
                | None -> return Results.NotFound()
    }