namespace FSharpEcommerce.Models

open System

/// Represents a standardized API error response
type ApiErrorResponse =
    { ErrorCode: string
      Message: string
      Details: Map<string, string> option
      Timestamp: DateTime }

/// Factory methods for creating consistent error responses
module ApiErrorResponse =
    /// Creates a standard API error response
    let create errorCode message details =
        { ErrorCode = errorCode
          Message = message
          Details = details
          Timestamp = DateTime.UtcNow }

    /// Creates a validation error response
    let validation message details =
        create "VALIDATION_ERROR" message (Some details)

    /// Creates an authentication error response
    let authentication message =
        create "AUTHENTICATION_ERROR" message None

    /// Creates an authorization error response
    let authorization message =
        create "AUTHORIZATION_ERROR" message None

    /// Creates a not found error response
    let notFound message = create "NOT_FOUND_ERROR" message None

    /// Creates a server error response
    let serverError message = create "SERVER_ERROR" message None

    /// Creates a bad request error response
    let badRequest message = create "BAD_REQUEST_ERROR" message None

    /// Creates a conflict error response
    let conflict message = create "CONFLICT_ERROR" message None
