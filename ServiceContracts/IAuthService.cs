

using ServiceContracts.DTOs.Responses;
using Services.DTOs.UserDTOs;

namespace ServiceContracts
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto registerRequestDto);

        Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token);

        Task<Result<AuthResponse>> ResendConfirmationEmailAsync(string email);
        Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
    }
}
