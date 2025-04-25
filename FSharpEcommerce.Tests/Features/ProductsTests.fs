namespace FSharpEcommerce.Tests.Features

open Xunit
open FSharpEcommerce.Tests.Setup
open System.Net
open System.Net.Http.Json
open FSharpEcommerce.Features.Products

type ProductsTests(fixture: CustomFixture) =
    inherit TestBase(fixture)

    [<Fact>]
    member this.``GetProducts - Should return list of products``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // Act
            let! response = client.GetAsync("/products")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetProductsResponse> response

            Assert.NotNull(result)
            Assert.NotNull(result.Products)
        }

    [<Fact>]
    member this.``GetProduct - Should return product when exists``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a product
            let createRequest =
                {| Name = "Test Product"
                   Description = "This is a test product description for testing"
                   Price = 19.99m
                   StockQuantity = 10
                   CategoryId = 1 |}

            let! createResponse = client.PostAsJsonAsync("/products", createRequest)

            let createdProduct =
                this.ToType<CreateProductResponse> createResponse

            // Act
            let! response = client.GetAsync($"/products/{createdProduct.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetProductResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdProduct.Id, result.Id)
            Assert.Equal(createRequest.Name, result.Name)
            Assert.Equal(createRequest.Description, result.Description)
            Assert.Equal(createRequest.Price, result.Price)
            Assert.Equal(createRequest.StockQuantity, result.StockQuantity)
            Assert.Equal(createRequest.CategoryId, result.CategoryId)
        }

    [<Fact>]
    member this.``GetProduct - Should return not found for non-existent product``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.GetAsync($"/products/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``CreateProduct - Should create a new product and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = "New Test Product"
                   Description = "This is a detailed description for the new test product"
                   Price = 29.99m
                   StockQuantity = 15
                   CategoryId = 1 |}

            // Act
            let! response = client.PostAsJsonAsync("/products", request)

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let result =
                this.ToType<CreateProductResponse> response

            Assert.NotNull(result)
            Assert.Equal(request.Name, result.Name)
            Assert.Equal(request.Description, result.Description)
            Assert.Equal(request.Price, result.Price)
            Assert.Equal(request.StockQuantity, result.StockQuantity)
            Assert.Equal(request.CategoryId, result.CategoryId)
            Assert.True(result.Id > 0)
        }

    [<Fact>]
    member this.``CreateProduct - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = ""
                   Description = "Too short"
                   Price = -1m
                   StockQuantity = -5
                   CategoryId = 0 |}

            // Act
            let! response = client.PostAsJsonAsync("/products", request)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateProduct - Should update existing product and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a product
            let createRequest =
                {| Name = "Product to Update"
                   Description = "This product will be updated in the test"
                   Price = 25.99m
                   StockQuantity = 20
                   CategoryId = 1 |}

            let! createResponse = client.PostAsJsonAsync("/products", createRequest)

            let createdProduct =
                this.ToType<CreateProductResponse> createResponse

            // Now update it
            let updateRequest =
                {| Name = "Updated Product Name"
                   Description = "This is the updated description for the test product"
                   Price = 39.99m
                   StockQuantity = 30
                   CategoryId = 2 |}

            // Act
            let! response = client.PutAsJsonAsync($"/products/{createdProduct.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<UpdateProductResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdProduct.Id, result.Id)
            Assert.Equal(updateRequest.Name, result.Name)
            Assert.Equal(updateRequest.Description, result.Description)
            Assert.Equal(updateRequest.Price, result.Price)
            Assert.Equal(updateRequest.StockQuantity, result.StockQuantity)
            Assert.Equal(updateRequest.CategoryId, result.CategoryId)
            Assert.NotNull(result.UpdatedAt)
        }

    [<Fact>]
    member this.``UpdateProduct - Should return not found for non-existent product``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            let updateRequest =
                {| Name = "Updated Name"
                   Description = "This is an updated description that will fail"
                   Price = 15.99m
                   StockQuantity = 25
                   CategoryId = 1 |}

            // Act
            let! response = client.PutAsJsonAsync($"/products/{nonExistentId}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateProduct - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a product
            let createRequest =
                {| Name = "Product for Validation"
                   Description = "This product will be used in validation test"
                   Price = 12.99m
                   StockQuantity = 8
                   CategoryId = 1 |}

            let! createResponse = client.PostAsJsonAsync("/products", createRequest)

            let createdProduct =
                this.ToType<CreateProductResponse> createResponse

            // Try to update with invalid data
            let updateRequest =
                {| Name = ""
                   Description = "Too short"
                   Price = -1m
                   StockQuantity = -5
                   CategoryId = 0 |}

            // Act
            let! response = client.PutAsJsonAsync($"/products/{createdProduct.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``DeleteProduct - Should delete existing product and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a product
            let createRequest =
                {| Name = "Product to Delete"
                   Description = "This product will be deleted in the test"
                   Price = 9.99m
                   StockQuantity = 5
                   CategoryId = 1 |}

            let! createResponse = client.PostAsJsonAsync("/products", createRequest)

            let createdProduct =
                this.ToType<CreateProductResponse> createResponse

            // Act
            let! response = client.DeleteAsync($"/products/{createdProduct.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            // Verify it's actually deleted
            let! getResponse = client.GetAsync($"/products/{createdProduct.Id}")
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode)
        }

    [<Fact>]
    member this.``DeleteProduct - Should return not found for non-existent product``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.DeleteAsync($"/products/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
