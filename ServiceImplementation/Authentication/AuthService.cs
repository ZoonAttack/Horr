using Entities;
using Entities.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using Services.DTOs.UserDTOs;
using Services.Interfaces;
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
        public AuthService(UserManager<Entities.Users.User> userManager, ITokenService tokenService, IEmailService emailService,AppDbContext context ,IConfiguration configuration)

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if(user.IsDeleted)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Account is deleted",
                    Errors = new List<string> { "The account associated with this email has been deleted." }
                };
            }
            
            // ... Validate Password Logic ...
            if (user is null || _userManager.CheckPasswordAsync(user, dto.Password).Result == false)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Invalid credentials",
                    Errors = new List<string> { "Email or Password is incorrect." }
                };
            }

            // ONE LINE to generate tokens now!
            var authResponse = await GenerateAuthResponseAsync(user);

            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = authResponse
            };
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto dto)
        {
            // 1. Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Email already in use."
                };
            }

            // 2. Create the User Entity
            var user = new Entities.Users.User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
                // Ensure Refresh Token is null initially
                //RefreshToken = null
            };

            // 3. Save to DB
            var createResult = await _userManager.CreateAsync(user, dto.Password);

            if (!createResult.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Errors = createResult.Errors.Select(e => e.Description).ToList()
                };
            }

            // 4. Assign Role (Using your Enum logic)
            // Ensure you handle the case where the Role might not exist in DB yet
            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role.ToString());

            if (!roleResult.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Errors = roleResult.Errors.Select(e => e.Description).ToList()
                };
            }
            try
            {
                bool result = await SendEmailHelperAsync(user);
                return new Result<AuthResponse>
                {
                    Succeeded = true, 
                    Data = new AuthResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        IsEmailConfirmationSent = result 
                    },
                    Message = result
                    ? "Registration successful. Please check your email."
                    : "Account created, but we failed to send the confirmation email. Please request a new one."
                };

            } catch(Exception ex)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Account created, but failed to send confirmation email.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "User not found."
                };
            }
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Email confirmation failed.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            var authResponse = await GenerateAuthResponseAsync(user);
            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = authResponse
            };
        }

        public async Task<Result<AuthResponse>> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
            {
                return new Result<AuthResponse>()
                {
                    Succeeded = false,
                    Message = "Invalid request.",
                    Errors = new List<string> { "User not found or already confirmed." }
                };
            }
            try { 
            bool result = await SendEmailHelperAsync(user);
                return new Result<AuthResponse>
                {
                    Succeeded = result,
                    Data = new AuthResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        IsEmailConfirmationSent = result
                    },
                    Message = result
                   ? "Confirmation email resent. Please check your inbox."
                   : "Failed to resend confirmation email. Please try again later."
                };
            } catch(Exception ex)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Failed to resend confirmation email. Please try again later.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            // 1. Find the token in the database
            // We include the User because we need to generate a new Access Token for them later
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == refreshToken);

            // 2. Validation Checks
            if (storedToken == null)
            {
                return new Result<AuthResponse> 
                { 
                    Succeeded = false,
                    Errors = new List<string> { "Token does not exist" }
                };
            }

            // Check 2a: Has it expired?
            if (storedToken.IsExpired)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Errors = new List<string> { "Token has expired" }
                };
            }

            // Check 2b: Has it already been revoked? (Replay Attack detection)
            // If a user tries to use a token that we already marked as "Revoked", it might be stolen.
            if (storedToken.Revoked != null)
            {
                // OPTIONAL SECURITY STEP: Revoke all tokens for this user chain
                // because we don't know who is the real user anymore.
                // await RevokeAllTokensForUser(storedToken.ApplicationUserId);

                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Errors = new List<string> { "Invalid Token" }
                };
            }

            // 3. Mark the current token as used (Revoke it)
            storedToken.Revoked = DateTime.UtcNow;

            // 4. Generate NEW tokens (Rotation)
            // Assuming you have a helper method to create the JWT string
            var newAccessToken = _tokenService.GenerateAccessToken(await GetUserClaimsAsync(storedToken.User));
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // 5. Create the new Refresh Token entity in DB
            var newTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = storedToken.UserId, // Link to same user
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7), // Match your cookie expiry                                        
            };

            // 6. Save changes
            // This updates the old token (RevokedOn) AND inserts the new one
            _context.RefreshTokens.Add(newTokenEntity);
            await _context.SaveChangesAsync();

            // 7. Return the result
            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = new AuthResponse
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            };
        }

        #region helper

        private async Task<bool> SendEmailHelperAsync(Entities.Users.User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // 3. Build URL
            var baseUrl = _configuration["AppURL"];
            var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?userId={user.Id}&token={encodedToken}";

            // 4. Send Email
            var message = $"<h1>Welcome!</h1><p>Please <a href='{confirmationLink}'>click here</a> to confirm your email.</p>";
            bool result = await _emailService.SendEmailAsync(user.Email, "Confirm your email", message);
            return result;
        }

        private async Task<List<Claim>> GetUserClaimsAsync(Entities.Users.User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            // Add role claims
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            return authClaims;
        }
        // Helper method to generate tokens and update the user in DB
        private async Task<AuthResponse> GenerateAuthResponseAsync(Entities.Users.User user)
        {
            var authClaims = await GetUserClaimsAsync(user);
            // 3. Generate Tokens
            var accessToken = _tokenService.GenerateAccessToken(authClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            // 5. Return the DTO
            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }
        #endregion
    }
}
