using System;
using System.Threading.Tasks;
using Entities.Users;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.DTOs.UserDTOs;

namespace ServiceContracts.Settings
{
    public interface IProfileSettings
    {
        Task<Result<UserProfileDto>> GetProfileAsync(string userId);
        Task<Result<UserProfileDto>> UpdateFullNameAsync(string userId, string newName);
        Task<Result<UserProfileDto>> UpdateEmailAsync(string userId, string newEmail);
        Task<Result<UserProfileDto>> UpdateTitleAsync(string userId, string newTitle);
        Task<Result<UserProfileDto>> UpdateBioAsync(string userId, string newBio);
        Task<Result<UserProfileDto>> UpdateExperienceLevelAsync(string userId, int experienceLevel);
        Task<Result<UserProfileDto>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto);

        //Task<Result<UserProfileDto>> UpdateBillingAsync(string userId, CreateBillingDTO);

        Task<Result<UserProfileDto>> UpdateAccountAsync(string userId, AccountUpdateDto dto);
        Task<Result<UserProfileDto>> UpdateLocationAsync(string userId, LocationUpdateDto dto);
        Task<Result<UserProfileDto>> GetPrivacySettingsAsync(string userId);
        Task<Result<UserProfileDto>> UpdatePrivacySettingsAsync(string userId, PrivacyUpdateDto dto);
    }
}
