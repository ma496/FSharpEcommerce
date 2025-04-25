namespace FSharpEcommerce.Tests.Features

open Xunit
open FSharpEcommerce.Tests.Setup
open System.Net
open System.Net.Http.Json
open FSharpEcommerce.Features.Categories

type CategoriesTests(fixture: Fixture) =
    inherit TestBase(fixture)

    [<Fact>]
    member this.``GetCategories - Should return list of categories``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // Act
            let! response = client.GetAsync("/categories")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetCategoriesResponse> response

            Assert.NotNull(result)
            Assert.NotNull(result.Categories)
        }

    [<Fact>]
    member this.``GetCategory - Should return category when exists``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a category
            let createRequest =
                {| Name = "Test Category"
                   Description = "This is a test category description for testing" |}

            let! createResponse = client.PostAsJsonAsync("/categories", createRequest)

            let createdCategory =
                this.ToType<CreateCategoryResponse> createResponse

            // Act
            let! response = client.GetAsync($"/categories/{createdCategory.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetCategoryResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdCategory.Id, result.Id)
            Assert.Equal(createRequest.Name, result.Name)
            Assert.Equal(createRequest.Description, result.Description)
        }

    [<Fact>]
    member this.``GetCategory - Should return not found for non-existent category``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.GetAsync($"/categories/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``CreateCategory - Should create a new category and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = "New Test Category"
                   Description = "This is a detailed description for the new test category" |}

            // Act
            let! response = client.PostAsJsonAsync("/categories", request)

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let result =
                this.ToType<CreateCategoryResponse> response

            Assert.NotNull(result)
            Assert.Equal(request.Name, result.Name)
            Assert.Equal(request.Description, result.Description)
            Assert.True(result.Id > 0)
        }

    [<Fact>]
    member this.``CreateCategory - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = ""
                   Description = "Too short" |}

            // Act
            let! response = client.PostAsJsonAsync("/categories", request)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateCategory - Should update existing category and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a category
            let createRequest =
                {| Name = "Category to Update"
                   Description = "This category will be updated in the test" |}

            let! createResponse = client.PostAsJsonAsync("/categories", createRequest)

            let createdCategory =
                this.ToType<CreateCategoryResponse> createResponse

            // Now update it
            let updateRequest =
                {| Name = "Updated Category Name"
                   Description = "This is the updated description for the test category" |}

            // Act
            let! response = client.PutAsJsonAsync($"/categories/{createdCategory.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<UpdateCategoryResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdCategory.Id, result.Id)
            Assert.Equal(updateRequest.Name, result.Name)
            Assert.Equal(updateRequest.Description, result.Description)
            Assert.NotNull(result.UpdatedAt)
        }

    [<Fact>]
    member this.``UpdateCategory - Should return not found for non-existent category``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            let updateRequest =
                {| Name = "Updated Name"
                   Description = "This is an updated description that will fail" |}

            // Act
            let! response = client.PutAsJsonAsync($"/categories/{nonExistentId}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateCategory - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a category
            let createRequest =
                {| Name = "Category for Validation"
                   Description = "This category will be used in validation test" |}

            let! createResponse = client.PostAsJsonAsync("/categories", createRequest)

            let createdCategory =
                this.ToType<CreateCategoryResponse> createResponse

            // Try to update with invalid data
            let updateRequest =
                {| Name = ""
                   Description = "Too short" |}

            // Act
            let! response = client.PutAsJsonAsync($"/categories/{createdCategory.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``DeleteCategory - Should delete existing category and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a category
            let createRequest =
                {| Name = "Category to Delete"
                   Description = "This category will be deleted in the test" |}

            let! createResponse = client.PostAsJsonAsync("/categories", createRequest)

            let createdCategory =
                this.ToType<CreateCategoryResponse> createResponse

            // Act
            let! response = client.DeleteAsync($"/categories/{createdCategory.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            // Verify it's actually deleted
            let! getResponse = client.GetAsync($"/categories/{createdCategory.Id}")
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode)
        }

    [<Fact>]
    member this.``DeleteCategory - Should return not found for non-existent category``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.DeleteAsync($"/categories/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
