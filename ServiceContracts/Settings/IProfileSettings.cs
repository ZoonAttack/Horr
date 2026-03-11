using System;
using System.Threading.Tasks;
using Entities.Users;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;

namespace ServiceContracts.Settings
{
    public interface IProfileSettings
    {
        Task<Result<User>> UpdateFullNameAsync(string userId, string newName);
        Task<Result<User>> UpdateEmailAsync(string userId, string newEmail);
        Task<Result<User>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto);

        //Task<Result<User>> UpdateBillingAsync(string userId, CreateBillingDTO);

        Task<Result<User>> UpdateAccountAsync(string userId, AccountUpdateDto dto);
        Task<Result<User>> UpdateLocationAsync(string userId, LocationUpdateDto dto);
        Task<Result<PrivacyResponseDto>> GetPrivacySettingsAsync(string userId);
        Task<Result<User>> UpdatePrivacySettingsAsync(string userId, PrivacyUpdateDto dto);
    }
}
