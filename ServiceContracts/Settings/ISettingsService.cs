using System;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Settings;

namespace ServiceContracts.Settings
{
    public interface ISettingsService
    {
        Task<bool> UpdateAccountAsync(Guid userId, AccountUpdateDto dto);
        Task<bool> UpdateLocationAsync(Guid userId, LocationUpdateDto dto);
        Task<PrivacyResponseDto?> GetPrivacySettingsAsync(Guid userId);
        Task<bool> UpdatePrivacySettingsAsync(Guid userId, PrivacyUpdateDto dto);
    }
}
