namespace FSharpEcommerce.Migrations

open FluentMigrator
open System.Data

[<Migration(20250426102L, "Create Orders Table")>]
type CreateOrdersTable() =
    inherit Migration()

    override _.Up() =
        base.Create.Table("Orders")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("PaymentMethod").AsString(50).NotNullable()
            .WithColumn("ShippingAddress").AsString(255).NotNullable()
            .WithColumn("BillingAddress").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Orders_Customers")
            .FromTable("Orders")
            .ForeignColumn("CustomerId")
            .ToTable("Customers")
            .PrimaryColumn("Id")
            .OnDelete Rule.Cascade
        |> ignore

    override _.Down() =
        base.Delete.Table("Orders") |> ignore
