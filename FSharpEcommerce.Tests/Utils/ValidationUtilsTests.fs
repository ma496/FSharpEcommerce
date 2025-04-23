module FSharpEcommerce.Tests.Utils.ValidationUtilsTests

open Expecto
open FSharpEcommerce.Utils
open FSharpEcommerce.Utils.ValidationUtils
open FSharpEcommerce.Utils.ValidationUtils.Validators

// Define test records for validation
type TestUser =
    { Name: string
      Email: string
      Age: int }

// Helper functions for testing
let createValidationError fieldName message =
    { FieldName = fieldName
      Message = message }

[<Tests>]
let basicValidatorsTests =
    testList
        "Basic Validators Tests"
        [ testCase "required - Valid input passes validation"
          <| fun _ ->
              let result = required "Name" "John"
              Expect.isOk result "Valid name should pass validation"

          testCase "required - Empty input fails validation"
          <| fun _ ->
              let result = required "Name" ""
              Expect.isError result "Empty name should fail validation"

              match result with
              | Error errors ->
                  Expect.equal errors.Length 1 "Should have one error"
                  Expect.equal errors.[0].FieldName "Name" "Field name should be 'Name'"
                  Expect.stringContains errors.[0].Message "required" "Error message should mention required"
              | Ok _ -> failtest "Should not be Ok"

          testCase "minLength - Valid input passes validation"
          <| fun _ ->
              let result =
                  minLength "Password" 6 "Secret123"

              Expect.isOk result "Valid password should pass validation"

          testCase "minLength - Too short input fails validation"
          <| fun _ ->
              let result = minLength "Password" 6 "123"
              Expect.isError result "Too short password should fail validation"

          testCase "maxLength - Valid input passes validation"
          <| fun _ ->
              let result =
                  maxLength "Description" 10 "Short desc"

              Expect.isOk result "Valid description should pass validation"

          testCase "maxLength - Too long input fails validation"
          <| fun _ ->
              let result =
                  maxLength "Description" 10 "This is a very long description that should fail validation"

              Expect.isError result "Too long description should fail validation"

          testCase "email - Valid email passes validation"
          <| fun _ ->
              let result =
                  email "Email" "test@example.com"

              Expect.isOk result "Valid email should pass validation"

          testCase "email - Invalid email fails validation"
          <| fun _ ->
              let result = email "Email" "invalid-email"
              Expect.isError result "Invalid email should fail validation"

          testCase "equal - Equal values pass validation"
          <| fun _ ->
              let result =
                  equal "Password" "Secret123" "Secret123"

              Expect.isOk result "Equal values should pass validation"

          testCase "equal - Different values fail validation"
          <| fun _ ->
              let result =
                  equal "Password" "Secret123" "Different"

              Expect.isError result "Different values should fail validation"

          testCase "notEqual - Different values pass validation"
          <| fun _ ->
              let result =
                  notEqual "NewPassword" "NewSecret" "OldSecret"

              Expect.isOk result "Different values should pass validation"

          testCase "notEqual - Equal values fail validation"
          <| fun _ ->
              let result =
                  notEqual "NewPassword" "Secret123" "Secret123"

              Expect.isError result "Equal values should fail validation"

          testCase "greaterThan - Greater value passes validation"
          <| fun _ ->
              let result = greaterThan "Age" 21 18
              Expect.isOk result "Greater value should pass validation"

          testCase "greaterThan - Equal value fails validation"
          <| fun _ ->
              let result = greaterThan "Age" 18 18
              Expect.isError result "Equal value should fail validation"

          testCase "greaterThan - Smaller value fails validation"
          <| fun _ ->
              let result = greaterThan "Age" 16 18
              Expect.isError result "Smaller value should fail validation"

          testCase "greaterThanOrEqual - Greater value passes validation"
          <| fun _ ->
              let result = greaterThanOrEqual "Age" 21 18
              Expect.isOk result "Greater value should pass validation"

          testCase "greaterThanOrEqual - Equal value passes validation"
          <| fun _ ->
              let result = greaterThanOrEqual "Age" 18 18
              Expect.isOk result "Equal value should pass validation"

          testCase "greaterThanOrEqual - Smaller value fails validation"
          <| fun _ ->
              let result = greaterThanOrEqual "Age" 16 18
              Expect.isError result "Smaller value should fail validation"

          testCase "lessThan - Smaller value passes validation"
          <| fun _ ->
              let result = lessThan "Age" 16 18
              Expect.isOk result "Smaller value should pass validation"

          testCase "lessThan - Equal value fails validation"
          <| fun _ ->
              let result = lessThan "Age" 18 18
              Expect.isError result "Equal value should fail validation"

          testCase "lessThan - Greater value fails validation"
          <| fun _ ->
              let result = lessThan "Age" 21 18
              Expect.isError result "Greater value should fail validation"

          testCase "lessThanOrEqual - Smaller value passes validation"
          <| fun _ ->
              let result = lessThanOrEqual "Age" 16 18
              Expect.isOk result "Smaller value should pass validation"

          testCase "lessThanOrEqual - Equal value passes validation"
          <| fun _ ->
              let result = lessThanOrEqual "Age" 18 18
              Expect.isOk result "Equal value should pass validation"

          testCase "lessThanOrEqual - Greater value fails validation"
          <| fun _ ->
              let result = lessThanOrEqual "Age" 21 18
              Expect.isError result "Greater value should fail validation"

          testCase "minValue - Valid value passes validation"
          <| fun _ ->
              let result = minValue "Quantity" 5 10
              Expect.isOk result "Valid quantity should pass validation"

          testCase "minValue - Too small value fails validation"
          <| fun _ ->
              let result = minValue "Quantity" 5 3
              Expect.isError result "Too small quantity should fail validation"

          testCase "maxValue - Valid value passes validation"
          <| fun _ ->
              let result = maxValue "Quantity" 10 5
              Expect.isOk result "Valid quantity should pass validation"

          testCase "maxValue - Too large value fails validation"
          <| fun _ ->
              let result = maxValue "Quantity" 10 15
              Expect.isError result "Too large quantity should fail validation" ]

[<Tests>]
let combinedValidatorsTests =
    testList
        "Combined Validators Tests"
        [ testCase "combine - All validations pass"
          <| fun _ ->
              let nameValidations =
                  [ required "Name"
                    minLength "Name" 2
                    maxLength "Name" 50 ]

              let result = combine nameValidations "John"
              Expect.isOk result "Valid name should pass all validations"

          testCase "combine - Some validations fail"
          <| fun _ ->
              let nameValidations =
                  [ required "Name"
                    minLength "Name" 5
                    maxLength "Name" 50 ]

              let result = combine nameValidations "Jo"
              Expect.isError result "Short name should fail minLength validation"

              match result with
              | Error errors ->
                  Expect.isGreaterThan errors.Length 0 "Should have errors"

                  Expect.isTrue
                      (errors
                       |> List.exists (fun e ->
                           e.FieldName = "Name"
                           && e.Message.Contains("least 5")))
                      "Should contain minLength error"
              | Ok _ -> failtest "Should not be Ok"

          testCase "combine - All validations fail"
          <| fun _ ->
              let nameValidations =
                  [ required "Name"
                    minLength "Name" 2
                    maxLength "Name" 5 ]

              let result =
                  combine nameValidations "This is too long"

              Expect.isError result "Long name should fail maxLength validation"

              match result with
              | Error errors ->
                  Expect.isGreaterThan errors.Length 0 "Should have errors"

                  Expect.isTrue
                      (errors
                       |> List.exists (fun e ->
                           e.FieldName = "Name"
                           && e.Message.Contains("cannot exceed")))
                      "Should contain maxLength error"
              | Ok _ -> failtest "Should not be Ok" ]

[<Tests>]
let validateFieldTests =
    testList
        "Validate Field Tests"
        [ testCase "validateField - All validations pass"
          <| fun _ ->
              let emailValidations =
                  [ required "Email"; email "Email" ]

              let result =
                  validateField "test@example.com" emailValidations

              Expect.isOk result "Valid email should pass all validations"

          testCase "validateField - Some validations fail"
          <| fun _ ->
              let emailValidations =
                  [ required "Email"; email "Email" ]

              let result =
                  validateField "not-an-email" emailValidations

              Expect.isError result "Invalid email should fail email validation"

              match result with
              | Error errors ->
                  Expect.isGreaterThan errors.Length 0 "Should have errors"

                  Expect.isTrue
                      (errors
                       |> List.exists (fun e ->
                           e.FieldName = "Email"
                           && e.Message.Contains("valid email")))
                      "Should contain email validation error"
              | Ok _ -> failtest "Should not be Ok" ]

[<Tests>]
let computationExpressionTests =
    testList
        "Computation Expression Tests"
        [ testCase "validate - All validations pass"
          <| fun _ ->
              // Define a validation workflow for a user
              let validateUser (user: TestUser) =
                  validate {
                      let! name =
                          validateField
                              user.Name
                              [ required "Name"
                                minLength "Name" 2
                                maxLength "Name" 50 ]

                      let! email = validateField user.Email [ required "Email"; email "Email" ]

                      let! age =
                          validateField
                              user.Age
                              [ minValue "Age" 18
                                maxValue "Age" 120 ]

                      return
                          { Name = name
                            Email = email
                            Age = age }
                  }

              let user =
                  { Name = "John"
                    Email = "john@example.com"
                    Age = 30 }

              let result = validateUser user

              Expect.isOk result "Valid user should pass all validations"

          testCase "validate - Some validations fail"
          <| fun _ ->
              // Define a validation workflow for a user
              let validateUser (user: TestUser) =
                  validate {
                      let! name =
                          validateField
                              user.Name
                              [ required "Name"
                                minLength "Name" 2
                                maxLength "Name" 50 ]

                      let! email = validateField user.Email [ required "Email"; email "Email" ]

                      let! age =
                          validateField
                              user.Age
                              [ minValue "Age" 18
                                maxValue "Age" 120 ]

                      return
                          { Name = name
                            Email = email
                            Age = age }
                  }

              let user =
                  { Name = "Jo"
                    Email = "not-an-email"
                    Age = 16 }

              let result = validateUser user

              Expect.isError result "Invalid user should fail validations"

              match result with
              | Error errors ->
                  Expect.isGreaterThan errors.Length 0 "Should have errors"
                  // Check that we have all expected errors
                  Expect.isTrue
                      (errors
                       |> List.exists (fun e ->
                           e.FieldName = "Email"
                           && e.Message.Contains("valid email")))
                      "Should contain email validation error"

                  Expect.isTrue
                      (errors
                       |> List.exists (fun e ->
                           e.FieldName = "Age"
                           && e.Message.Contains("at least 18")))
                      "Should contain age validation error"
              | Ok _ -> failtest "Should not be Ok" ]
