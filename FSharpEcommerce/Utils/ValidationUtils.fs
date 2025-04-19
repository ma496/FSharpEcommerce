namespace FSharpEcommerce.Utils

open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open System

// Define our own validation types
type ValidationError = { FieldName: string; Message: string }
type ValidationResult<'T> = Result<'T, ValidationError list>

/// Utility functions for request validation
module ValidationUtils =
    /// Converts validation errors to a consistent error response format
    let private mapValidationErrors (validationErrors: ValidationError list) =
        validationErrors
        |> List.groupBy (fun e -> e.FieldName)
        |> List.map (fun (key, errors) -> (key, errors |> List.map (fun e -> e.Message) |> String.concat "; "))
        |> Map.ofList

    /// Validates the request with the provided validator and executes the handler if valid
    let validateRequest<'T> (validate: 'T -> ValidationResult<'T>) (request: 'T) (handler: 'T -> Task<IResult>) =
        task {
            match validate request with
            | Ok validatedRequest -> return! handler validatedRequest
            | Error validationErrors ->
                let errorsMap = mapValidationErrors validationErrors
                return ResultUtils.validationError "Validation failed" errorsMap
        }

    /// Basic validation functions
    module Validators =
        /// Simple validation for required string
        let required fieldName (value: string) =
            if String.IsNullOrWhiteSpace(value) then
                Error
                    [ { FieldName = fieldName
                        Message = sprintf "%s is required" fieldName } ]
            else
                Ok value

        /// Simple validation for minimum string length
        let minLength fieldName minLength (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif value.Length < minLength then
                Error
                    [ { FieldName = fieldName
                        Message = sprintf "%s must be at least %d characters" fieldName minLength } ]
            else
                Ok value

        /// Simple validation for maximum string length
        let maxLength fieldName maxLength (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif value.Length > maxLength then
                Error
                    [ { FieldName = fieldName
                        Message = sprintf "%s cannot exceed %d characters" fieldName maxLength } ]
            else
                Ok value

        /// Simple email validation
        let email fieldName (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif not (value.Contains("@") && value.Contains(".")) then
                Error
                    [ { FieldName = fieldName
                        Message = sprintf "%s must be a valid email address" fieldName } ]
            else
                Ok value

        /// Simple validation for minimum numeric value
        let minValue<'T when 'T: comparison> fieldName (minValue: 'T) (value: 'T) =
            if value < minValue then
                Error
                    [ { FieldName = fieldName
                        Message = sprintf "%s must be at least %A" fieldName minValue } ]
            else
                Ok value

        /// Combine validation functions for a value
        let combine (validations: ('T -> ValidationResult<'T>) list) (value: 'T) =
            let rec loop validations value errors =
                match validations with
                | [] ->
                    if List.isEmpty errors then
                        Ok value
                    else
                        Error(List.concat errors)
                | validate :: rest ->
                    match validate value with
                    | Ok _ -> loop rest value errors
                    | Error err -> loop rest value (err :: errors)

            loop validations value []

    /// Create a validate computation expression for chaining validation functions
    type ValidateBuilder() =
        member _.Bind(m: ValidationResult<'T>, f: 'T -> ValidationResult<'U>) : ValidationResult<'U> =
            match m with
            | Ok value -> f value
            | Error errors -> Error errors

        member _.Return(value: 'T) : ValidationResult<'T> = Ok value

        member _.ReturnFrom(m: ValidationResult<'T>) : ValidationResult<'T> = m

    let validate = ValidateBuilder()
