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
                        Message = $"%s{fieldName} is required" } ]
            else
                Ok value

        /// Simple validation for minimum string length
        let minLength fieldName minLength (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif value.Length < minLength then
                Error
                    [ { FieldName = fieldName
                        Message = $"%s{fieldName} must be at least %d{minLength} characters" } ]
            else
                Ok value

        /// Simple validation for maximum string length
        let maxLength fieldName maxLength (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif value.Length > maxLength then
                Error
                    [ { FieldName = fieldName
                        Message = $"%s{fieldName} cannot exceed %d{maxLength} characters" } ]
            else
                Ok value

        /// Simple email validation
        let email fieldName (value: string) =
            if String.IsNullOrEmpty(value) then
                Ok value // Skip this check if already empty (rely on required check)
            elif not (value.Contains("@") && value.Contains(".")) then
                Error
                    [ { FieldName = fieldName
                        Message = $"%s{fieldName} must be a valid email address" } ]
            else
                Ok value

        /// Simple validation for minimum numeric value
        let minValue<'T when 'T: comparison> fieldName (minValue: 'T) (value: 'T) =
            if value < minValue then
                Error
                    [ { FieldName = fieldName
                        Message = $"%s{fieldName} must be at least %A{minValue}" } ]
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

    /// Helper functions for field validation within computation expressions
    let validateField (field: 'T) (validations: ('T -> ValidationResult<'T>) list) : ValidationResult<'T> =
        let results = validations |> List.map (fun v -> v field)

        let errors =
            results
            |> List.choose (function
                | Error errs -> Some errs
                | Ok _ -> None)
            |> List.concat

        if List.isEmpty errors then Ok field else Error errors

    // Type to accumulate all validation results
    type ValidationState<'T> =
        { Value: 'T
          Errors: ValidationError list }

        static member Success(value: 'T) = { Value = value; Errors = [] }
        static member WithErrors(value: 'T, errors) = { Value = value; Errors = errors }

        static member AddErrors(state: ValidationState<'T>, errors) =
            { state with
                Errors = state.Errors @ errors }

        member this.ToResult() =
            if List.isEmpty this.Errors then
                Ok this.Value
            else
                Error this.Errors

    // Computation expression that collects all errors
    type ValidateBuilder() =
        member _.Bind(m: ValidationResult<'T>, f: 'T -> ValidationState<'U>) : ValidationState<'U> =
            match m with
            | Ok value -> f value
            | Error errors ->
                // We need a default state to collect errors
                let defaultState = f Unchecked.defaultof<'T>

                { defaultState with
                    Errors = errors @ defaultState.Errors }

        member _.Return(value: 'T) : ValidationState<'T> = ValidationState.Success(value)

        member _.ReturnFrom(state: ValidationState<'T>) : ValidationState<'T> = state

        member _.ReturnFrom(result: ValidationResult<'T>) : ValidationState<'T> =
            match result with
            | Ok value -> ValidationState.Success(value)
            | Error errors -> ValidationState.WithErrors(Unchecked.defaultof<'T>, errors)

        member _.Zero() : ValidationState<unit> = ValidationState.Success(())

        member _.Combine(state1: ValidationState<unit>, state2: ValidationState<'T>) : ValidationState<'T> =
            { state2 with
                Errors = state1.Errors @ state2.Errors }

        member _.Delay(f: unit -> ValidationState<'T>) : unit -> ValidationState<'T> = f

        member _.Run(f: unit -> ValidationState<'T>) : ValidationResult<'T> =
            let state = f ()
            state.ToResult()

        // Let expressions for field validation
        member this.Let(value: 'T, f: 'T -> ValidationState<'U>) : ValidationState<'U> = f value

        member this.LetField
            (value: 'T, validations: ('T -> ValidationResult<'T>) list, projection: 'T -> ValidationState<'U>)
            : ValidationState<'U> =
            let results = validations |> List.map (fun v -> v value)

            let errors =
                results
                |> List.choose (function
                    | Error errs -> Some errs
                    | Ok _ -> None)
                |> List.concat

            let nextState = projection value

            if List.isEmpty errors then
                nextState
            else
                { nextState with
                    Errors = errors @ nextState.Errors }

    let validate = ValidateBuilder()
