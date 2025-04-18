namespace FSharpEcommerce.Services

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Tokens
open FSharpEcommerce.Models
open System.Threading.Tasks

type IJwtService =
    abstract member GenerateToken: User -> Role list -> Task<string>
    abstract member ValidateToken: string -> ClaimsPrincipal option

type JwtService(jwtSettings: JwtSettings) =
    interface IJwtService with
        member this.GenerateToken (user: User) (roles: Role list) =
            task {
                let claims =
                    [ Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
                      Claim(JwtRegisteredClaimNames.Email, user.Email)
                      Claim(ClaimTypes.Name, user.Username)
                      // Add roles as claims
                      yield! roles |> List.map (fun role -> Claim(ClaimTypes.Role, role.Name)) ]

                let key = SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                let credentials = SigningCredentials(key, SecurityAlgorithms.HmacSha256)

                let tokenDescriptor =
                    SecurityTokenDescriptor(
                        Subject = ClaimsIdentity(claims),
                        Expires = Nullable(DateTime.UtcNow.AddMinutes(float jwtSettings.ExpiryMinutes)),
                        SigningCredentials = credentials,
                        Issuer = jwtSettings.Issuer,
                        Audience = jwtSettings.Audience
                    )

                let tokenHandler = JwtSecurityTokenHandler()
                let token = tokenHandler.CreateToken(tokenDescriptor)
                return tokenHandler.WriteToken(token)
            }

        member this.ValidateToken(token: string) =
            let tokenHandler = JwtSecurityTokenHandler()

            let validationParameters =
                TokenValidationParameters(
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                )

            try
                let principal =
                    tokenHandler.ValidateToken(token, validationParameters, ref Unchecked.defaultof<SecurityToken>)

                Some principal
            with _ ->
                None
