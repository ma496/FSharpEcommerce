namespace FSharpEcommerce.Models

[<CLIMutable>]
type User =
    { Id: int
      Username: string
      Email: string
      PasswordHash: string
      CreatedAt: System.DateTime }

[<CLIMutable>]
type Role = { Id: int; Name: string }

[<CLIMutable>]
type UserRole = { UserId: int; RoleId: int }

type JwtSettings =
    { Secret: string
      Issuer: string
      Audience: string
      ExpiryMinutes: int }

// Create a module with factory function for JwtSettings
module JwtSettings =
    let create secret issuer audience expiryMinutes =
        { Secret = secret
          Issuer = issuer
          Audience = audience
          ExpiryMinutes = expiryMinutes }
