namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250418102L, "Create Roles table")>]
type CreateRolesTable() =
    inherit Migration()
    override _.Up() =
        base.Create.Table("Roles")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(50).NotNullable().Unique()
        |> ignore

    override _.Down() =
        base.Delete.Table("Roles") |> ignore
