namespace FSharpEcommerce.Tests.Features

open Xunit
open FSharpEcommerce.Tests.Setup
open System.Net
open System.Net.Http.Json
open FSharpEcommerce.Features.Customers

type CustomerTests(fixture: CustomFixture) =
    inherit TestBase(fixture)

    [<Fact>]
    member this.``GetCustomers - Should return list of customers``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // Act
            let! response = client.GetAsync("/customers")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetCustomersResponse> response

            Assert.NotNull(result)
            Assert.NotNull(result.Customers)
        }

    [<Fact>]
    member this.``GetCustomer - Should return customer when exists``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a customer
            let createRequest =
                {| Name = "Test Customer"
                   Email = "test@example.com"
                   Phone = "123-456-7890"
                   Address = "123 Test St"
                   City = "Test City"
                   State = "TS"
                   ZipCode = "12345"
                   Country = "Test Country" |}

            let! createResponse = client.PostAsJsonAsync("/customers", createRequest)

            let createdCustomer =
                this.ToType<CreateCustomerResponse> createResponse

            // Act
            let! response = client.GetAsync($"/customers/{createdCustomer.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetCustomerResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdCustomer.Id, result.Id)
            Assert.Equal(createRequest.Name, result.Name)
            Assert.Equal(createRequest.Email, result.Email)
            Assert.Equal(createRequest.Phone, result.Phone)
            Assert.Equal(createRequest.Address, result.Address)
            Assert.Equal(createRequest.City, result.City)
            Assert.Equal(createRequest.State, result.State)
            Assert.Equal(createRequest.ZipCode, result.ZipCode)
            Assert.Equal(createRequest.Country, result.Country)
        }

    [<Fact>]
    member this.``GetCustomer - Should return not found for non-existent customer``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.GetAsync($"/customers/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``CreateCustomer - Should create a new customer and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = "New Test Customer"
                   Email = "newtest@example.com"
                   Phone = "987-654-3210"
                   Address = "456 New St"
                   City = "New City"
                   State = "NS"
                   ZipCode = "54321"
                   Country = "New Country" |}

            // Act
            let! response = client.PostAsJsonAsync("/customers", request)

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let result =
                this.ToType<CreateCustomerResponse> response

            Assert.NotNull(result)
            Assert.Equal(request.Name, result.Name)
            Assert.Equal(request.Email, result.Email)
            Assert.Equal(request.Phone, result.Phone)
            Assert.Equal(request.Address, result.Address)
            Assert.Equal(request.City, result.City)
            Assert.Equal(request.State, result.State)
            Assert.Equal(request.ZipCode, result.ZipCode)
            Assert.Equal(request.Country, result.Country)
            Assert.True(result.Id > 0)
        }

    [<Fact>]
    member this.``CreateCustomer - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| Name = ""
                   Email = "invalid-email"
                   Phone = ""
                   Address = ""
                   City = ""
                   State = ""
                   ZipCode = ""
                   Country = "" |}

            // Act
            let! response = client.PostAsJsonAsync("/customers", request)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateCustomer - Should update existing customer and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a customer
            let createRequest =
                {| Name = "Customer to Update"
                   Email = "update@example.com"
                   Phone = "111-222-3333"
                   Address = "Update Street"
                   City = "Update City"
                   State = "US"
                   ZipCode = "11111"
                   Country = "Update Country" |}

            let! createResponse = client.PostAsJsonAsync("/customers", createRequest)

            let createdCustomer =
                this.ToType<CreateCustomerResponse> createResponse

            // Now update it
            let updateRequest =
                {| Name = "Updated Customer Name"
                   Email = "updated@example.com"
                   Phone = "444-555-6666"
                   Address = "Updated Address"
                   City = "Updated City"
                   State = "UD"
                   ZipCode = "22222"
                   Country = "Updated Country" |}

            // Act
            let! response = client.PutAsJsonAsync($"/customers/{createdCustomer.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<UpdateCustomerResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdCustomer.Id, result.Id)
            Assert.Equal(updateRequest.Name, result.Name)
            Assert.Equal(updateRequest.Email, result.Email)
            Assert.Equal(updateRequest.Phone, result.Phone)
            Assert.Equal(updateRequest.Address, result.Address)
            Assert.Equal(updateRequest.City, result.City)
            Assert.Equal(updateRequest.State, result.State)
            Assert.Equal(updateRequest.ZipCode, result.ZipCode)
            Assert.Equal(updateRequest.Country, result.Country)
            Assert.NotNull(result.UpdatedAt)
        }

    [<Fact>]
    member this.``UpdateCustomer - Should return not found for non-existent customer``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            let updateRequest =
                {| Name = "Updated Name"
                   Email = "updated@example.com"
                   Phone = "444-555-6666"
                   Address = "Updated Address"
                   City = "Updated City"
                   State = "UD"
                   ZipCode = "22222"
                   Country = "Updated Country" |}

            // Act
            let! response = client.PutAsJsonAsync($"/customers/{nonExistentId}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateCustomer - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a customer
            let createRequest =
                {| Name = "Customer for Validation"
                   Email = "validation@example.com"
                   Phone = "777-888-9999"
                   Address = "Validation Street"
                   City = "Validation City"
                   State = "VS"
                   ZipCode = "33333"
                   Country = "Validation Country" |}

            let! createResponse = client.PostAsJsonAsync("/customers", createRequest)

            let createdCustomer =
                this.ToType<CreateCustomerResponse> createResponse

            // Try to update with invalid data
            let updateRequest =
                {| Name = ""
                   Email = "invalid-email"
                   Phone = ""
                   Address = ""
                   City = ""
                   State = ""
                   ZipCode = ""
                   Country = "" |}

            // Act
            let! response = client.PutAsJsonAsync($"/customers/{createdCustomer.Id}", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``DeleteCustomer - Should delete existing customer and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create a customer
            let createRequest =
                {| Name = "Customer to Delete"
                   Email = "delete@example.com"
                   Phone = "000-000-0000"
                   Address = "Delete Street"
                   City = "Delete City"
                   State = "DS"
                   ZipCode = "00000"
                   Country = "Delete Country" |}

            let! createResponse = client.PostAsJsonAsync("/customers", createRequest)

            let createdCustomer =
                this.ToType<CreateCustomerResponse> createResponse

            // Act
            let! deleteResponse = client.DeleteAsync($"/customers/{createdCustomer.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode)

            // Verify it's actually deleted
            let! getResponse = client.GetAsync($"/customers/{createdCustomer.Id}")
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode)
        }

    [<Fact>]
    member this.``DeleteCustomer - Should return not found for non-existent customer``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            let client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.DeleteAsync($"/customers/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }