

using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.User;

namespace ServiceContracts
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<Result<AuthResponse>> RegisterAsync(RegisterRequestDto registerRequestDto);

        Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token);
    }
}
