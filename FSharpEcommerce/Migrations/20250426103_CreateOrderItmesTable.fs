namespace FSharpEcommerce.Migrations

open FluentMigrator
open System.Data

[<Migration(20250426103L, "Create Order Items Table")>]
type CreateOrderItemsTable() =
    inherit Migration()

    override _.Up() =
        base.Create.Table("OrderItems")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("OrderId").AsInt32().NotNullable()
            .WithColumn("ProductId").AsInt32().NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
        |> ignore

        base.Create
            .ForeignKey("FK_OrderItems_Orders")
            .FromTable("OrderItems")
            .ForeignColumn("OrderId")
            .ToTable("Orders")
            .PrimaryColumn("Id")
            .OnDelete(Rule.Cascade)
        |> ignore

        base.Create
            .ForeignKey("FK_OrderItems_Products")
            .FromTable("OrderItems")
            .ForeignColumn("ProductId")
            .ToTable("Products")
            .PrimaryColumn("Id")
        |> ignore

    override _.Down() =
        base.Delete.Table("OrderItems") |> ignore

