namespace FSharpEcommerce.Migrations

open FluentMigrator
open Microsoft.FSharp.Linq

[<Migration(20250420102L, "Create Products Table")>]
type CreateProductsTable() =
    inherit Migration()

    override _.Up() =
        base.Create.Table("Products")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable().Unique()
            .WithColumn("Description").AsString(255).Nullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("StockQuantity").AsInt32().NotNullable()
            .WithColumn("CategoryId").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Products_Categories")
            .FromTable("Products")
            .ForeignColumn("CategoryId")
            .ToTable("Categories")
            .PrimaryColumn("Id")
        |> ignore

    override _.Down() =
        base.Delete.ForeignKey("FK_Products_Categories").OnTable("Products") |> ignore
        base.Delete.Table("Products") |> ignore
