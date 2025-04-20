namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250420101L, "Create Categories Table")>]
type CreateCategoriesTable() =
    inherit Migration()

    override _.Up() =
        base.Create.Table("Categories")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable().Unique()
            .WithColumn("Description").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().NotNullable()
        |> ignore
        
    override _.Down() =
        base.Delete.Table("Users") |> ignore
