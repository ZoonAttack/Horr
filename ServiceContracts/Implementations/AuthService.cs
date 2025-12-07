using Entities.User;
using Microsoft.AspNetCore.Identity;
using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Services.Implementations
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService; // <--- Inject this

        public AuthService(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // ... Validate Password Logic ...
            if(user is null || _userManager.CheckPasswordAsync(user, dto.Password).Result == false)
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
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
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

            // 5. GENERATE TOKENS (Auto-Login)
            var authResponse = await GenerateAuthResponseAsync(user);

            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = authResponse,
                Message = "User registered and logged in successfully"
            };
        }


        #region helper

        // Helper method to generate tokens and update the user in DB
        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
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
