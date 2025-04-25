namespace FSharpEcommerce.Tests.Setup

open Xunit

[<CollectionDefinition("CollectionFixture")>]
type CollectionFixture() =
    interface ICollectionFixture<Fixture>
