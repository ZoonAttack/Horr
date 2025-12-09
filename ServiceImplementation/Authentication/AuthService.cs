using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Services.DTOs.UserDTOs;
namespace ServiceImplementation.Authentication
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<Entities.Users.User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<Entities.Users.User> userManager, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)

// ... rest of the file remains unchanged ...
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

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
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // 3. Build URL
            var baseUrl = _configuration["AppURL"];
            var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?userId={user.Id}&token={encodedToken}";

            // 4. Send Email
            var message = $"<h1>Welcome!</h1><p>Please <a href='{confirmationLink}'>click here</a> to confirm your email.</p>";
            bool result = await _emailService.SendEmailAsync(user.Email, "Confirm your email", message);
            if (result == false)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Failed to send confirmation email."
                };
            }
            return new Result<AuthResponse>
            {
                Succeeded = true,
                Message = "Registration successful. Please check your email to confirm your account."
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
        #region helper

        // Helper method to generate tokens and update the user in DB
        private async Task<AuthResponse> GenerateAuthResponseAsync(Entities.Users.User user)
        {
            // 1. Get Roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // 2. Build Claims (Same as Login)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", user.Id)
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Generate Tokens
            var accessToken = _tokenService.GenerateAccessToken(authClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 4. Update User with Refresh Token
            //user.RefreshToken = refreshToken;
            //user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

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
