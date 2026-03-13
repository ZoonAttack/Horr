using Entities; // Using the AppDbContext directly for simplicity if a specific UserRepository doesn't encompass all updates
using Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.Settings;
using Services.Implementations;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Settings
{
    public class ProfileSettings : IProfileSettings
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService; 

        public ProfileSettings(UserManager<User> userManager, EmailService emailService, AppDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }
        public async Task<Result<User>> UpdateFullNameAsync(string userId, string newName)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.IsDeleted) return new Result<User>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update full name.",
                Data = null
            };

            user.FullName = newName;
            _userManager.UpdateAsync(user).Wait();

            return new Result<User>
            {
                Succeeded = true,
                Errors = { },
                Message = "Full name updated successfully.",
                Data = user
            };
        }

        public async Task<Result<User>> UpdateEmailAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<User>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update email.",
                Data = null
            };

            var confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var emailSent = await _emailService.SendConfirmationEmailAsync(userId, newEmail, confirmationToken);

            return emailSent
                ? new Result<User>
                {
                    Succeeded = true,
                    Errors = { },
                    Message = "Confirmation email sent to new address. Please confirm to complete the update.",
                    Data = user
                }
                : new Result<User>
                {
                    Succeeded = false,
                    Errors = { "Failed to send confirmation email." },
                    Message = "Failed to update email.",
                    Data = null
                };
        }


        public async Task<Result<User>> UpdateAccountAsync(string userId, AccountUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted) return new Result<User>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update account settings."
            };

            // Partial update: only update if value is provided
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpper();
                user.UserName = dto.Email;
                user.NormalizedUserName = dto.Email.ToUpper();
            }

            if (dto.Bio != null)
                user.Bio = dto.Bio;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return new Result<User>
            {
                Succeeded = true,
                Errors = { },
                Message = "Account settings updated successfully.",
                Data = user
            };
        }

        public async Task<Result<User>> UpdateLocationAsync(string userId, LocationUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted) return new Result<User>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update location settings."
            };

            // Partial update
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.City != null) user.City = dto.City;
            if (dto.StateProvince != null) user.StateProvince = dto.StateProvince;
            if (dto.ZipCode != null) user.ZipCode = dto.ZipCode;
            if (dto.Country != null) user.Country = dto.Country;
            if (dto.TimeZone != null) user.TimeZone = dto.TimeZone;
            
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return new Result<User>
            {
                Succeeded = true,
                Errors = { },
                Message = "Location settings updated successfully.",
                Data = user
            };
        }

        public async Task<Result<PrivacyResponseDto>> GetPrivacySettingsAsync(string userId)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return null;

            // Requirement: HTML Prototype User ID Hash (e83b2bbd) -> Use first 8 chars of GUID for representation
            string hash = userId.Substring(0, 8);

            return new Result<PrivacyResponseDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Privacy settings retrieved successfully.",
                Data = new PrivacyResponseDto
                {
                    UserIdHash = hash,
                    Visibility = freelancer.VisibilityPreference,
                    ExperienceLevel = freelancer.ExperienceLevel
                }
            };
        }

        public async Task<Result<User>> UpdatePrivacySettingsAsync(string userId, PrivacyUpdateDto dto)
        {
            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<User>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update privacy settings."
            };

            if (dto.Visibility.HasValue)
                freelancer.VisibilityPreference = dto.Visibility.Value;

            if (dto.ExperienceLevel.HasValue)
                freelancer.ExperienceLevel = dto.ExperienceLevel.Value;

            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();
            return new Result<User>
            {
                Succeeded = true,
                Errors = { },
                Message = "Privacy settings updated successfully.",
                Data = await _context.Users.FindAsync(userId)
            };
        }

        public async Task<Result<User>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if(user == null || user.IsDeleted) return new Result<User>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to add payment method."
            };

            await _context.PaymentMethods.AddAsync(dto.ToPaymentMethod(userId));
            await _context.SaveChangesAsync();
            return new Result<User>
            {
                Succeeded = true,
                Errors = { },
                Message = "Payment method added successfully.",
                Data = user //This needs to change to a more appropriate DTO that includes the new payment method details,
                            //but for simplicity, I'm returning the user here. In a real implementation,
                            //a UserDTO with payment method details should be returned.
                            //This goes for all methods that currently return User,
                            //they should ideally return a more specific DTO that includes the relevant details for the operation performed.
            };
        }
    }
}
