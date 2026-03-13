using ServiceContracts.DTOs.Responses;
using Services.DTOs.Authentication;
using Services.DTOs.UserDTOs;

namespace Services.Authentication
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto registerRequestDto);

        Task<Result<AuthResponse>> ChangeEmailAsync(string userId, string newEmail, string token);

        Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token);

        Task<Result<AuthResponse>> ResendConfirmationEmailAsync(string email);
        Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
        Task<Result<AuthResponse>> ChangePasswordAsync(string userId, ChangePasswordRequestDTO dto);
    }
}
