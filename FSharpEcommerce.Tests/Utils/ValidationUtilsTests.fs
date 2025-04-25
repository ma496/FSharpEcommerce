namespace FSharpEcommerce.Tests.Utils

open Xunit
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

type ValidationUtilsTests() =
    [<Fact>]
    member this.``ValidateUser - Should return errors for invalid input``() =
        task {
            // Arrange & Act
            let result =
                validate {
                    let! _ =
                        validateField
                            "abc12"
                            [ required "Username"
                              minLength "Username" 3 ]

                    let! _ = validateField "invalid" [ required "Email"; email "Email" ]

                    let! _ =
                        validateField
                            "123"
                            [ required "Password"
                              minLength "Password" 8 ]

                    return ()
                }

            // Assert
            match result with
            | Error errors ->
                Assert.Equal(2, errors.Length)
                Assert.Equal("Email", errors[0].FieldName)
                Assert.Equal("Password", errors[1].FieldName)
            | Ok _ -> Assert.True(false, "Expected validation error")
        }
