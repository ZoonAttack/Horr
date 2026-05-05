using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.UserDTOs;

namespace ServiceContracts.Client
{
    public interface IClientProfileService
    {
        Task<Result<ClientMeDto>> GetClientMeAsync(string userId);
        Task<Result<ClientOnboardingDto>> GetClientOnboardingAsync(string userId);
    }
}
