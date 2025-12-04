using Entities.User;
using Microsoft.AspNetCore.Identity;
using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.User;


namespace Services.Implementations
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        public AuthService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            // Validate user credentials using UserManager
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.Email);
            
            // If valid, generate JWT and Refresh Token

            // Return AuthResponse DTO with Success Result
            throw new NotImplementedException();
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            // 1. Create User
            var result = await _userManager.CreateAsync(user, dto.Password);

            // 2. CHECK FOR FAILURE (Fixing the bug in your snippet)
            if (!result.Succeeded)
            {
                return new Result<AuthResponse>
                {
                    Succeeded = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // 3. Add Role
            // Ensure the role exists before adding, or this might throw an error.
            await _userManager.AddToRoleAsync(user, dto.Role.ToString());

            // 4. Return Success Data
            //Cookie is sent through the controller to the browser
            return new Result<AuthResponse>
            {
                Succeeded = true,
                Data = new AuthResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                },
                Message = "User registered successfully"
            };
        }
    }
}
