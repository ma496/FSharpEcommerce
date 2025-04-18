namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250418101L, "Create Users table")>]
type CreateUsersTable() =
    inherit Migration()
    override _.Up() =
        base.Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Username").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("PasswordHash").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
        |> ignore

    override _.Down() =
        base.Delete.Table("Users") |> ignore
