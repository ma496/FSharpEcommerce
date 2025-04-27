namespace FSharpEcommerce.Tests.Features

open Xunit
open FSharpEcommerce.Tests.Setup
open System.Net
open System.Net.Http.Json
open FSharpEcommerce.Features.Orders
open System

type OrderTests(fixture: CustomFixture) =
    inherit TestBase(fixture)

    [<Fact>]
    member this.``GetOrders - Should return list of orders``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // Act
            let! response = client.GetAsync("/orders")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetOrdersResponse> response

            Assert.NotNull(result)
            Assert.NotNull(result.Orders)
        }

    [<Fact>]
    member this.``GetOrder - Should return order when exists``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create an order
            let createRequest =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 100.00m
                   PaymentMethod = "Credit Card"
                   ShippingAddress = "123 Test St, Test City, TS 12345"
                   BillingAddress = "123 Test St, Test City, TS 12345"
                   Items =
                    [
                        {| ProductId = 1
                           Quantity = 2
                           Price = 50.00m |}
                    ] |}

            let! createResponse = client.PostAsJsonAsync("/orders", createRequest)

            let createdOrder =
                this.ToType<CreateOrderResponse> createResponse

            // Act
            let! response = client.GetAsync($"/orders/{createdOrder.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<GetOrderResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdOrder.Id, result.Id)
            Assert.Equal(createRequest.CustomerId, result.CustomerId)
            Assert.Equal(createRequest.TotalAmount, result.TotalAmount)
            Assert.Equal(createRequest.PaymentMethod, result.PaymentMethod)
            Assert.Equal(createRequest.ShippingAddress, result.ShippingAddress)
            Assert.Equal(createRequest.BillingAddress, result.BillingAddress)
            Assert.NotNull(result.Items)
            Assert.NotEmpty(result.Items)
        }

    [<Fact>]
    member this.``GetOrder - Should return not found for non-existent order``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.GetAsync($"/orders/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``CreateOrder - Should create a new order and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 150.00m
                   PaymentMethod = "PayPal"
                   ShippingAddress = "456 New St, New City, NS 54321"
                   BillingAddress = "456 New St, New City, NS 54321"
                   Items =
                    [
                        {| ProductId = 1
                           Quantity = 1
                           Price = 50.00m |}
                        {| ProductId = 2
                           Quantity = 2
                           Price = 50.00m |}
                    ] |}

            // Act
            let! response = client.PostAsJsonAsync("/orders", request)

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let result =
                this.ToType<CreateOrderResponse> response

            Assert.NotNull(result)
            Assert.Equal(request.CustomerId, result.CustomerId)
            Assert.Equal(request.TotalAmount, result.TotalAmount)
            Assert.Equal(request.PaymentMethod, result.PaymentMethod)
            Assert.Equal(request.ShippingAddress, result.ShippingAddress)
            Assert.Equal(request.BillingAddress, result.BillingAddress)
            Assert.Equal("Pending", result.Status)
            Assert.NotNull(result.Items)
            Assert.Equal(2, result.Items.Length)
            Assert.True(result.Id > 0)
        }

    [<Fact>]
    member this.``CreateOrder - Should validate input``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let request =
                {| CustomerId = 0
                   OrderDate = DateTime.MinValue
                   TotalAmount = -10.00m
                   PaymentMethod = ""
                   ShippingAddress = ""
                   BillingAddress = ""
                   Items = [] |}

            // Act
            let! response = client.PostAsJsonAsync("/orders", request)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateOrderStatus - Should update existing order status and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create an order
            let createRequest =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 200.00m
                   PaymentMethod = "Credit Card"
                   ShippingAddress = "789 Status St, Status City, SS 99999"
                   BillingAddress = "789 Status St, Status City, SS 99999"
                   Items =
                    [
                        {| ProductId = 3
                           Quantity = 2
                           Price = 100.00m |}
                    ] |}

            let! createResponse = client.PostAsJsonAsync("/orders", createRequest)

            let createdOrder =
                this.ToType<CreateOrderResponse> createResponse

            // Now update status
            let updateRequest =
                {| OrderId = createdOrder.Id
                   Status = "Processing" |}

            // Act
            let! response = client.PostAsJsonAsync("/orders/update-status", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<UpdateOrderStatusResponse> response

            Assert.NotNull(result)
            Assert.Equal(createdOrder.Id, result.OrderId)
            Assert.Equal("Processing", result.Status)

            // Verify the status was actually updated
            let! getResponse = client.GetAsync($"/orders/{createdOrder.Id}")
            let order = this.ToType<GetOrderResponse> getResponse
            Assert.Equal("Processing", order.Status)
        }

    [<Fact>]
    member this.``UpdateOrderStatus - Should return not found for non-existent order``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            let updateRequest =
                {| OrderId = nonExistentId
                   Status = "Processing" |}

            // Act
            let! response = client.PostAsJsonAsync("/orders/update-status", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``UpdateOrderStatus - Should validate status transitions``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create an order
            let createRequest =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 75.00m
                   PaymentMethod = "Credit Card"
                   ShippingAddress = "321 Invalid St, Invalid City, IS 55555"
                   BillingAddress = "321 Invalid St, Invalid City, IS 55555"
                   Items =
                    [
                        {| ProductId = 1
                           Quantity = 1
                           Price = 75.00m |}
                    ] |}

            let! createResponse = client.PostAsJsonAsync("/orders", createRequest)

            let createdOrder =
                this.ToType<CreateOrderResponse> createResponse

            // Try to update with invalid transition (Pending -> Shipped)
            let updateRequest =
                {| OrderId = createdOrder.Id
                   Status = "Shipped" |}

            // Act
            let! response = client.PostAsJsonAsync("/orders/update-status", updateRequest)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``DeleteOrder - Should delete pending order and return success``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create an order
            let createRequest =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 125.00m
                   PaymentMethod = "Credit Card"
                   ShippingAddress = "654 Delete St, Delete City, DS 77777"
                   BillingAddress = "654 Delete St, Delete City, DS 77777"
                   Items =
                    [
                        {| ProductId = 2
                           Quantity = 1
                           Price = 125.00m |}
                    ] |}

            let! createResponse = client.PostAsJsonAsync("/orders", createRequest)

            let createdOrder =
                this.ToType<CreateOrderResponse> createResponse

            // Act
            let! deleteResponse = client.DeleteAsync($"/orders/{createdOrder.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode)

            // Verify it's actually deleted
            let! getResponse = client.GetAsync($"/orders/{createdOrder.Id}")
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode)
        }

    [<Fact>]
    member this.``DeleteOrder - Should return not found for non-existent order``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            let nonExistentId = 9999

            // Act
            let! response = client.DeleteAsync($"/orders/{nonExistentId}")

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member this.``DeleteOrder - Should not allow deletion of orders in processing status``() =
        task {
            // Arrange
            let loginResponse = this.AdminLogin()

            use client =
                this.CreateAuthenticatedClient(loginResponse.Token)

            // First create an order
            let createRequest =
                {| CustomerId = 1
                   OrderDate = DateTime.UtcNow
                   TotalAmount = 175.00m
                   PaymentMethod = "Credit Card"
                   ShippingAddress = "987 Processing St, Processing City, PS 88888"
                   BillingAddress = "987 Processing St, Processing City, PS 88888"
                   Items =
                    [
                        {| ProductId = 3
                           Quantity = 1
                           Price = 175.00m |}
                    ] |}

            let! createResponse = client.PostAsJsonAsync("/orders", createRequest)

            let createdOrder =
                this.ToType<CreateOrderResponse> createResponse

            // Update to Processing
            let updateRequest =
                {| OrderId = createdOrder.Id
                   Status = "Processing" |}

            let! _ = client.PostAsJsonAsync("/orders/update-status", updateRequest)

            // Try to delete
            let! deleteResponse = client.DeleteAsync($"/orders/{createdOrder.Id}")

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode)
        }