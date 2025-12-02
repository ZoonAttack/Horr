using ServiceContracts;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class AuthService : IAuthService
    {
        public Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            // Validate user credentials using UserManager

            // If valid, generate JWT and Refresh Token

            // Return AuthResponse DTO with Success Result
            throw new NotImplementedException();
        }

        public Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            // Map DTO to Entity

            // Create user using UserManager (Give corresponding role)

            // Return AuthResponse DTO with Success Result

            throw new NotImplementedException();
        }
    }
}
