namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250418104L, "Add default roles")>]
type AddDefaultRoles() =
    inherit Migration()

    override _.Up() =
        // Insert default roles
        base.Insert.IntoTable("Roles").Row(dict [ "Name", "Admin" :> obj ]) |> ignore

        base.Insert.IntoTable("Roles").Row(dict [ "Name", "User" :> obj ]) |> ignore

    override _.Down() =
        base.Execute.Sql("DELETE FROM \"Roles\" WHERE \"Name\" = 'Admin'") |> ignore
        base.Execute.Sql("DELETE FROM \"Roles\" WHERE \"Name\" = 'User'") |> ignore
