namespace FSharpEcommerce.Utils

open System.Text.Json
open Microsoft.AspNetCore.Http
open FSharpEcommerce.Models

#nowarn "3391"

/// Utility functions for creating consistent HTTP results
module ResultUtils =
    /// Serialization options with camelCase property naming
    let private jsonOptions =
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options

    /// Creates a success response with the given data
    let success statusCode data =
        Results.Json(data, jsonOptions, statusCode = statusCode)

    /// Creates a success response with 200 OK status
    let ok data = success StatusCodes.Status200OK data

    /// Creates a success response with 201 Created status
    let created resourceUrl data =
        // For simplicity, just return a JSON response with 201 status code
        // In a real app, you should find a way to set the Location header
        success StatusCodes.Status201Created data

    /// Creates a success response with 204 No Content status
    let noContent = Results.NoContent()

    /// Creates an error response with the given status code and error details
    let error statusCode errorResponse =
        Results.Json(errorResponse, jsonOptions, statusCode = statusCode)

    /// Creates a bad request (400) error response
    let badRequest message =
        error StatusCodes.Status400BadRequest (ApiErrorResponse.badRequest message)

    /// Creates a validation error (400) response with field-specific details
    let validationError message details =
        error StatusCodes.Status400BadRequest (ApiErrorResponse.validation message details)

    /// Creates an unauthorized (401) error response
    let unauthorized message =
        error StatusCodes.Status401Unauthorized (ApiErrorResponse.authentication message)

    /// Creates a forbidden (403) error response
    let forbidden message =
        error StatusCodes.Status403Forbidden (ApiErrorResponse.authorization message)

    /// Creates a not found (404) error response
    let notFound message =
        error StatusCodes.Status404NotFound (ApiErrorResponse.notFound message)

    /// Creates a conflict (409) error response
    let conflict message =
        error StatusCodes.Status409Conflict (ApiErrorResponse.conflict message)

    /// Creates a server error (500) response
    let serverError message =
        error StatusCodes.Status500InternalServerError (ApiErrorResponse.serverError message)
