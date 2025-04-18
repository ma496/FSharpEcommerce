namespace FSharpEcommerce.Extensions

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Routing
open System
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.CompilerServices

type RoleAuthorizationFilter(roles: string list) =
    let hasRole (context: EndpointFilterInvocationContext) =
        let user = context.HttpContext.User

        if not user.Identity.IsAuthenticated then
            false
        else
            roles |> List.exists user.IsInRole

    interface IEndpointFilter with
        member this.InvokeAsync(context, next) =
            if hasRole context then
                next.Invoke(context)
            else
                Results.Forbid() |> ValueTask<obj>

[<Extension>]
type RouteHandlerBuilderExtensions() =
    [<Extension>]
    static member RequireRoles(builder: RouteHandlerBuilder, roles: string list) =
        builder.AddEndpointFilter(RoleAuthorizationFilter(roles))

    // Extension method for a single role
    [<Extension>]
    static member RequireRole(builder: RouteHandlerBuilder, role: string) = builder.RequireRoles([ role ])
