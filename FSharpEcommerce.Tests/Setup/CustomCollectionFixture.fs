namespace FSharpEcommerce.Tests.Setup

open Xunit

[<CollectionDefinition("CustomCollectionFixture")>]
type CustomCollectionFixture() =
    interface ICollectionFixture<CustomFixture>
