namespace FSharpEcommerce.Migrations

open FluentMigrator

[<Migration(20250426101L, "Create Customers Table")>]
type CreateCustomersTable() =
    inherit Migration()

    override _.Up() =
        base.Create.Table("Customers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(100).Nullable()
            .WithColumn("Phone").AsString(20).Nullable()
            .WithColumn("Address").AsString(255).Nullable()
            .WithColumn("City").AsString(100).Nullable()
            .WithColumn("State").AsString(100).Nullable()
            .WithColumn("ZipCode").AsString(20).Nullable()
            .WithColumn("Country").AsString(100).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
        |> ignore

    override _.Down() =
        base.Delete.Table("Customers") |> ignore
