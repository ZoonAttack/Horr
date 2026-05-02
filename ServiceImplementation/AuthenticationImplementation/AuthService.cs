using Entities;
using Entities.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using Services.Authentication;
using Services.DTOs.Authentication;
using Services.DTOs.UserDTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServiceImplementation.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Entities.Users.User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthService(UserManager<Entities.Users.User> userManager, ITokenService tokenService, IEmailService emailService, AppDbContext context, IConfiguration configuration)
        {
            _userManager   = userManager;
            _tokenService  = tokenService;
            _emailService  = emailService;
            _context       = context;
            _configuration = configuration;
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidCredentials,
                    Message   = "Invalid credentials.",
                    Errors    = new List<string> { "Email or Password is incorrect." }
                };
            }

            if (user.IsDeleted)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message   = "Account is deleted.",
                    Errors    = new List<string> { "The account associated with this email has been deleted." }
                };
            }

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidCredentials,
                    Message   = "Invalid credentials.",
                    Errors    = new List<string> { "Email or Password is incorrect." }
                };
            }

            if (!user.EmailConfirmed)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailNotConfirmed,
                    Message   = "Email not confirmed.",
                    Errors    = new List<string> { "Please confirm your email before logging in." }
                };
            }

            var authResponse = await GenerateAuthResponseAsync(user);

            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data      = authResponse
            };
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailAlreadyInUse,
                    Message   = "Email already in use."
                };
            }

            var user = new Entities.Users.User
            {
                UserName      = dto.Email,
                Email         = dto.Email,
                FullName      = dto.FullName,
                PhoneNumber   = dto.PhoneNumber,
                Bio           = dto.Bio           ?? string.Empty,
                Address       = dto.Address       ?? string.Empty,
                City          = dto.City          ?? string.Empty,
                Country       = dto.Country       ?? string.Empty,
                StateProvince = dto.StateProvince ?? string.Empty,
                TimeZone      = dto.TimeZone      ?? "UTC+02:00",
                ZipCode       = dto.ZipCode       ?? string.Empty
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.RegistrationFailed,
                    Errors    = createResult.Errors.Select(e => e.Description).ToList()
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role.ToString());
            if (!roleResult.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.RoleAssignmentFailed,
                    Errors    = roleResult.Errors.Select(e => e.Description).ToList()
                };
            }

            try
            {
                bool sent = await SendEmailHelperAsync(user);
                return new Result<AuthResponse>
                {
                    Succeeded = true,
                    Data = new AuthResponse
                    {
                        Id                      = user.Id,
                        Email                   = user.Email,
                        IsEmailConfirmationSent = sent,
                        isEmailConfirmed        = false
                    },
                    Message = sent
                        ? "Registration successful. Please check your email."
                        : "Account created, but we failed to send the confirmation email. Please request a new one."
                };
            }
            catch (Exception ex)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailSendFailed,
                    Message   = "Account created, but failed to send confirmation email.",
                    Errors    = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Result<AuthResponse>> ChangeEmailAsync(string userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "User not found."
                };
            }

            var changeEmailResult = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!changeEmailResult.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.TokenInvalid,
                    Message   = "Failed to update email.",
                    Errors    = changeEmailResult.Errors.Select(e => e.Description).ToList()
                };
            }

            var confirmationToken  = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationResult = await _userManager.ConfirmEmailAsync(user, confirmationToken);

            return new Result<AuthResponse>
            {
                Succeeded = true,
                Message   = confirmationResult.Succeeded
                    ? "Email updated and confirmed successfully."
                    : "Email updated but confirmation failed.",
                Data = new AuthResponse
                {
                    Id               = user.Id,
                    Email            = newEmail,
                    isEmailConfirmed = confirmationResult.Succeeded
                }
            };
        }

        public async Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "User not found."
                };
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result       = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailConfirmFailed,
                    Message   = "Email confirmation failed.",
                    Errors    = result.Errors.Select(e => e.Description).ToList()
                };
            }

            var authResponse = await GenerateAuthResponseAsync(user);
            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data      = authResponse
            };
        }

        public async Task<Result<AuthResponse>> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Invalid request.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            if (user.EmailConfirmed)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AlreadyConfirmed,
                    Message   = "Invalid request.",
                    Errors    = new List<string> { "Email is already confirmed." }
                };
            }

            try
            {
                bool sent = await SendEmailHelperAsync(user);
                return new Result<AuthResponse>
                {
                    Succeeded = sent,
                    ErrorCode = sent ? null : ErrorCodes.EmailSendFailed,
                    Data = new AuthResponse
                    {
                        Id                      = user.Id,
                        Email                   = user.Email,
                        IsEmailConfirmationSent = sent
                    },
                    Message = sent
                        ? "Confirmation email resent. Please check your inbox."
                        : "Failed to resend confirmation email. Please try again later."
                };
            }
            catch (Exception ex)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailSendFailed,
                    Message   = "Failed to resend confirmation email. Please try again later.",
                    Errors    = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == refreshToken);

            if (storedToken == null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.TokenInvalid,
                    Message   = "Token not found.",
                    Errors    = new List<string> { "The provided refresh token does not exist." }
                };
            }

            if (storedToken.IsExpired)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.TokenExpired,
                    Message   = "Token has expired.",
                    Errors    = new List<string> { "Please log in again." }
                };
            }

            if (storedToken.Revoked != null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.TokenInvalid,
                    Message   = "Token has been revoked.",
                    Errors    = new List<string> { "This token is no longer valid." }
                };
            }

            // Rotate: mark old token as used, generate new pair
            storedToken.Revoked = DateTime.UtcNow;

            var newAccessToken  = _tokenService.GenerateAccessToken(await GetUserClaimsAsync(storedToken.User));
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token   = newRefreshToken,
                UserId  = storedToken.UserId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = new AuthResponse
                {
                    Token        = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            };
        }

        public async Task<Result<AuthResponse>> ChangePasswordAsync(string userId, ChangePasswordRequestDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "User not found."
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

            return result.Succeeded
                ? new Result<AuthResponse>
                {
                    Succeeded = true,
                    Message   = "Password changed successfully."
                }
                : new Result<AuthResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.PasswordChangeFailed,
                    Message   = "Failed to change password. Current password may be incorrect.",
                    Errors    = result.Errors.Select(e => e.Description).ToList()
                };
        }

        #region helpers

        private async Task<bool> SendEmailHelperAsync(Entities.Users.User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return await _emailService.SendConfirmationEmailAsync(user.Id, user.Email, token);
        }

        private async Task<List<Claim>> GetUserClaimsAsync(Entities.Users.User user)
        {
            var userRoles   = await _userManager.GetRolesAsync(user);
            var authClaims  = new List<Claim>
            {
                new Claim(ClaimTypes.Name,           user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            return authClaims;
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(Entities.Users.User user)
        {
            var authClaims   = await GetUserClaimsAsync(user);
            var accessToken  = _tokenService.GenerateAccessToken(authClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Id               = user.Id,
                Email            = user.Email,
                Token            = accessToken,
                RefreshToken     = refreshToken,
                isEmailConfirmed = user.EmailConfirmed
            };
        }

        #endregion
    }
}
