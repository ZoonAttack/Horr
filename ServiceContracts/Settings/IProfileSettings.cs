using System;
using System.Threading.Tasks;
using Entities.Users;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.DTOs.UserDTOs;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;

namespace ServiceContracts.Settings
{
    public interface IProfileSettings
    {
        Task<Result<UserProfileDto>> GetProfileAsync(string userId);
        Task<Result<PublicProfileDto>> GetPublicProfileAsync(string userIdHash);
        Task<Result<string>> UpdateFullNameAsync(string userId, string newName);
        Task<Result<string>> UpdateEmailAsync(string userId, string newEmail);
        Task<Result<string>> UpdateTitleAsync(string userId, string newTitle);
        Task<Result<string?>> UpdateBioAsync(string userId, string? newBio);
        Task<Result<string>> UpdatePreferredCurrencyAsync(string userId, string preferredCurrency);
        Task<Result<ExperienceUpdateDto>> UpdateExperienceAsync(string userId, ExperienceUpdateDto dto);
        Task<Result<PaymentMethodReadDTO>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto);
        Task<Result<PaymentMethodReadDTO>> UpdateBillingAsync(string userId, string billingId, PaymentMethodUpdateDTO dto);

        Task<Result<bool>> DeleteBillingAsync(string userId, string id);

        Task<Result<AccountUpdateDto>> UpdateAccountAsync(string userId, AccountUpdateDto dto);
        Task<Result<LocationUpdateDto>> UpdateLocationAsync(string userId, LocationUpdateDto dto);
        Task<Result<FreelancerReadDTO>> UpdateFreelancerDetailsAsync(string userId, FreelancerUpdateDTO updateDto);
        Task<Result<UserProfileDto>> GetFreelancerDetailsAsync(string userId);
    }
}
