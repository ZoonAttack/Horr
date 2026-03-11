using System;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Settings;

namespace ServiceContracts.Settings
{
    public interface IProfileSettings
    {
        Task<bool> UpdateFullNameAsync(string userId, string newName);
        Task<bool> UpdateEmailAsync(string userId, string newEmail);
        Task<bool> UpdateAccountAsync(Guid userId, AccountUpdateDto dto);
        Task<bool> UpdateLocationAsync(Guid userId, LocationUpdateDto dto);
        Task<PrivacyResponseDto?> GetPrivacySettingsAsync(Guid userId);
        Task<bool> UpdatePrivacySettingsAsync(Guid userId, PrivacyUpdateDto dto);
    }
}
