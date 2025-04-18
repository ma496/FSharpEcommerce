namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250418103L, "Create UserRoles join table")>]
type CreateUserRolesTable() =
    inherit Migration()  

    override _.Up() =
        // 1. Create the join table
        base.Create.Table("UserRoles")
            .WithColumn("UserId").AsInt32().NotNullable() 
            .WithColumn("RoleId").AsInt32().NotNullable() 
        |> ignore

        // 2. Composite primary key over both columns
        base.Create.PrimaryKey("PK_UserRoles")
            .OnTable("UserRoles")
            .Columns([|"UserId"; "RoleId"|])      
        |> ignore

        // 3. Foreign key to Users
        base.Create.ForeignKey("FK_UserRoles_Users")
            .FromTable("UserRoles").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
        |> ignore

        // 4. Foreign key to Roles
        base.Create.ForeignKey("FK_UserRoles_Roles")
            .FromTable("UserRoles").ForeignColumn("RoleId")
            .ToTable("Roles").PrimaryColumn("Id")
        |> ignore

    override _.Down() =
        // Drop join table on rollback
        base.Delete.Table("UserRoles") |> ignore


