namespace FSharpEcommerce.Services

open System.Threading.Tasks
open FSharpEcommerce.Models
open FSharpEcommerce.Repositories
open BCrypt.Net

type IAuthService =
    abstract member Login: LoginRequest -> Task<AuthResponse option>
    abstract member Register: RegisterRequest -> Task<AuthResponse>

type AuthService(userRepository: IUserRepository, jwtService: IJwtService) =
    interface IAuthService with
        member this.Login(request: LoginRequest) =
            task {
                let! userOption = userRepository.GetUserByEmail(request.Email)

                match userOption with
                | None -> return None
                | Some user ->
                    let isPasswordValid = BCrypt.Verify(request.Password, user.PasswordHash)

                    if not isPasswordValid then
                        return None
                    else
                        let! roles = userRepository.GetUserRoles(user.Id)
                        let! token = jwtService.GenerateToken user roles

                        return
                            Some
                                { Token = token
                                  User = user
                                  Roles = roles }
            }

        member this.Register(request: RegisterRequest) =
            task {
                let passwordHash = BCrypt.HashPassword(request.Password)
                let! user = userRepository.CreateUser request passwordHash
                let! roles = userRepository.GetUserRoles(user.Id)
                let! token = jwtService.GenerateToken user roles

                return
                    { Token = token
                      User = user
                      Roles = roles }
            }
