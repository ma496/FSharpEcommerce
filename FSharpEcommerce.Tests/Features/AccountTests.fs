namespace FSharpEcommerce.Tests.Features

open Xunit
open FSharpEcommerce.Tests.Setup
open System.Net
open System.Net.Http.Json
open FSharpEcommerce.Features.Account
open System.Net.Http.Headers

type AccountTests(fixture: Fixture) =
    inherit TestBase(fixture)

    [<Fact>]
    member this.``Register - Should create a new user and return success``() =
        task {
            // Arrange
            let client = this.CreateClient()

            let randomEmail =
                System.Guid.NewGuid().ToString "N"
                + "@example.com"

            let request =
                {| Username =
                    "testuser_"
                    + System.Guid.NewGuid().ToString("N").Substring(0, 8)
                   Email = randomEmail
                   Password = "Password123!" |}

            // Act
            let! response = client.PostAsJsonAsync("/account/register", request)

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let result =
                this.ToType<RegisterResponse> response

            Assert.NotNull(result)
            Assert.Equal(request.Email, result.User.Email)
            Assert.Equal(request.Username, result.User.Username)
            Assert.NotEmpty(result.Token)
            Assert.NotEmpty(result.User.Roles)
        }

    [<Fact>]
    member this.``Register - Should return conflict for existing email``() =
        task {
            // Arrange
            let client = this.CreateClient()
            let email = "duplicate@example.com"

            // First registration
            let request1 =
                {| Username = "testuser1"
                   Email = email
                   Password = "Password123!" |}

            let! _ = client.PostAsJsonAsync("/account/register", request1)

            // Second registration with same email
            let request2 =
                {| Username = "testuser2"
                   Email = email
                   Password = "Password123!" |}

            // Act
            let! response = client.PostAsJsonAsync("/account/register", request2)

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode)
        }

    [<Fact>]
    member this.``Register - Should validate input``() =
        task {
            // Arrange
            let client = this.CreateClient()

            let request =
                {| Username = ""
                   Email = "invalid-email"
                   Password = "short" |}

            // Act
            let! response = client.PostAsJsonAsync("/account/register", request)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``Login - Should authenticate valid user and return token``() =
        task {
            // Arrange
            let client = this.CreateClient()
            let email = "login-test@example.com"
            let password = "Password123!"

            // Create user first
            let registerRequest =
                {| Username = "logintest"
                   Email = email
                   Password = password |}

            let! _ = client.PostAsJsonAsync("/account/register", registerRequest)

            // Now try to login
            let loginRequest =
                {| Email = email; Password = password |}

            // Act
            let! response = client.PostAsJsonAsync("/account/login", loginRequest)

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let result =
                this.ToType<LoginResponse> response

            Assert.NotNull(result)
            Assert.Equal(email, result.User.Email)
            Assert.NotEmpty(result.Token)
        }

    [<Fact>]
    member this.``Login - Should fail with invalid credentials``() =
        task {
            // Arrange
            let client = this.CreateClient()

            let loginRequest =
                {| Email = "nonexistent@example.com"
                   Password = "InvalidPassword123!" |}

            // Act
            let! response = client.PostAsJsonAsync("/account/login", loginRequest)

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member this.``Me - Should return user profile when authenticated``() =
        task {
            // Arrange
            let client = this.CreateClient()

            // Register a new user
            let email = "me-test@example.com"
            let password = "Password123!"
            let username = "metest"

            let registerRequest =
                {| Username = username
                   Email = email
                   Password = password |}

            let! registerResponse = client.PostAsJsonAsync("/account/register", registerRequest)

            let result =
                this.ToType<RegisterResponse> registerResponse

            // Set token in auth header
            let client2 = this.CreateClient()
            client2.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", result.Token)

            // Act
            let! response = client2.GetAsync("/account/me")

            let meResult =
                this.ToType<MeResponse> response

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)


            Assert.NotNull(meResult)
            Assert.Equal(email, meResult.User.Email)
            Assert.Equal(username, meResult.User.Username)
        }

    [<Fact>]
    member this.``Me - Should return unauthorized when not authenticated``() =
        task {
            // Arrange
            let client = this.CreateClient()

            // Act
            let! response = client.GetAsync("/account/me")

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
