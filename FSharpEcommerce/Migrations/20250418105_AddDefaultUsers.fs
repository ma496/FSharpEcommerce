namespace FSharpEcommerce.Migrations

open FluentMigrator
open BCrypt.Net

[<Migration(20250418105L, "Add default users")>]
type AddDefaultUsers() =
    inherit Migration()

    override _.Up() =
        // Admin user
        let adminPasswordHash = BCrypt.HashPassword("Admin123!")

        base.Insert
            .IntoTable("Users")
            .Row(
                dict
                    [ "Username", "admin" :> obj
                      "Email", "admin@example.com" :> obj
                      "PasswordHash", adminPasswordHash :> obj
                      "CreatedAt", System.DateTime.UtcNow :> obj ]
            )
        |> ignore

        // Get the admin user Id using SQL
        base.Execute.Sql(
            """
            INSERT INTO "UserRoles" ("UserId", "RoleId")
            SELECT 
                (SELECT "Id" FROM "Users" WHERE "Email" = 'admin@example.com'),
                (SELECT "Id" FROM "Roles" WHERE "Name" = 'Admin')
            """
        )
        |> ignore

        // Also give admin the User role
        base.Execute.Sql(
            """
            INSERT INTO "UserRoles" ("UserId", "RoleId")
            SELECT 
                (SELECT "Id" FROM "Users" WHERE "Email" = 'admin@example.com'),
                (SELECT "Id" FROM "Roles" WHERE "Name" = 'User')
            """
        )
        |> ignore

        // Regular user
        let userPasswordHash = BCrypt.HashPassword("User123!")

        base.Insert
            .IntoTable("Users")
            .Row(
                dict
                    [ "Username", "user" :> obj
                      "Email", "user@example.com" :> obj
                      "PasswordHash", userPasswordHash :> obj
                      "CreatedAt", System.DateTime.UtcNow :> obj ]
            )
        |> ignore

        // Assign User role
        base.Execute.Sql(
            """
            INSERT INTO "UserRoles" ("UserId", "RoleId")
            SELECT 
                (SELECT "Id" FROM "Users" WHERE "Email" = 'user@example.com'),
                (SELECT "Id" FROM "Roles" WHERE "Name" = 'User')
            """
        )
        |> ignore

    override _.Down() =
        // Remove users
        base.Execute.Sql(
            "DELETE FROM \"UserRoles\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"Email\" IN ('admin@example.com', 'user@example.com'))"
        )
        |> ignore

        base.Execute.Sql("DELETE FROM \"Users\" WHERE \"Email\" IN ('admin@example.com', 'user@example.com')")
        |> ignore
