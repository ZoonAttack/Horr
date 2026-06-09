using Entities;
using Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.Settings;
using ServiceContracts.DTOs.UserDTOs;
using ServiceImplementation.Helpers;
using Services.Authentication;
using Services.Implementations;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTOs.FreelancerProfile;
using Services.Freelancer;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Settings
{
    public class ProfileSettings : IProfileSettings
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IFreelancerService _freelancerService;

        public ProfileSettings(UserManager<User> userManager, IEmailService emailService, AppDbContext context, IFreelancerService freelancerService)
        {
            _userManager  = userManager;
            _emailService = emailService;
            _context      = context;
            _freelancerService = freelancerService;
        }

        private async Task<bool> ResolveAndSyncVerificationAsync(User user)
        {
            if (user.IsVerified)
            {
                return true;
            }

            var hasApprovedVerification = await _context.VerificationRequests
                .AsNoTracking()
                .AnyAsync(r => r.UserId == user.Id && r.Status == VerificationStatus.Approved);

            if (!hasApprovedVerification)
            {
                return false;
            }

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);
            return true;
        }
        
        public async Task<Result<UserProfileDto>> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to retrieve profile.",
                Data = null
            };

            // Retrieve role-specific freelancer profile information if they are a freelancer
            Freelancer? freelancer = null;
            if (user.Role == UserRole.Freelancer)
            {
                freelancer = await _context.Freelancers
                    .Include(f => f.Languages)
                    .Include(f => f.Education)
                    .Include(f => f.ExperienceDetails)
                    .Include(f => f.EmploymentHistory)
                    .FirstOrDefaultAsync(f => f.UserId == userId);
            }

            // Retrieve wallet balance
            var wallet = await _context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == userId);
            var balance = wallet?.BalanceEGP ?? 0m;

            // Retrieve payment methods
            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId)
                .ToListAsync();
            var paymentMethodDtos = paymentMethods.Select(pm => pm.ToPaymentMethodRead()).ToList();

            // Retrieve notifications flag
            var hasNotifications = await _context.Messages
                .AnyAsync(m => m.Status == MessageStatus.Unread && m.SenderId != userId &&
                               (m.Chat.ClientId == userId || m.Chat.FreelancerId == userId));

            var isVerified = await ResolveAndSyncVerificationAsync(user);
            var profileDto = user.ToUserProfileDto(
                freelancer: freelancer, 
                balance: balance, 
                paymentMethods: paymentMethodDtos,
                hasNotifications: hasNotifications);
            profileDto.IsVerified = isVerified;

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Profile retrieved successfully.",
                Data = profileDto
            };
        }

        public async Task<Result<PublicProfileDto>> GetPublicProfileAsync(string userIdHash)
        {
            var user = await _context.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.FreelancerSkills)
                        .ThenInclude(fs => fs.Skill)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.PortfolioItems)
                        .ThenInclude(pi => pi.Media)
                .FirstOrDefaultAsync(u => u.Id.StartsWith(userIdHash));

            if (user == null || user.IsDeleted) return new Result<PublicProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to retrieve public profile.",
                Data = null
            };

            var freelancer = user.Freelancer;
            var isVerified = await ResolveAndSyncVerificationAsync(user);

            var workHistory = await _context.Contracts
                .Include(c => c.ContractReviews)
                .Include(c => c.JobPost)
                .Where(c => c.FreelancerId == user.Id && (c.Status == ContractStatus.Completed || c.Status == ContractStatus.Closed))
                .Select(c => new EmploymentDto
                {
                    Company = "HORR Platform",
                    Title = c.JobPost != null ? c.JobPost.Title : (c.CustomJobDescription ?? "Contract"),
                    DateRange = $"{c.StartedAt:MMM yyyy} - {(c.ClosedAt.HasValue ? c.ClosedAt.Value.ToString("MMM yyyy") : "Present")}",
                    Description = c.ContractReviews.FirstOrDefault(r => r.ReviewerId != user.Id) != null 
                        ? c.ContractReviews.FirstOrDefault(r => r.ReviewerId != user.Id)!.Comment 
                        : ""
                }).ToListAsync();

            var publicProfile = new PublicProfileDto
            {
                FullName = user.FullName,
                Title = freelancer?.Title,
                Bio = user.Bio,
                City = user.City,
                Country = user.Country,
                ProfilePicturePath = user.ProfilePicturePath,
                TrustScore = user.TrustScore,
                IsVerified = isVerified,
                ExperienceLevel = (int)(freelancer?.ExperienceLevel ?? Entities.Enums.ExperienceLevel.Beginner),
                YearsOfExperience = freelancer?.YearsOfExperience,

                TotalEarnings = "$0", 
                TotalJobs = workHistory.Count,
                TotalHours = 0,

                Skills = freelancer?.FreelancerSkills.Select(s => s.Skill.Name).ToList() ?? new List<string>(),
                Portfolio = freelancer?.PortfolioItems.Where(pi => !pi.IsDeleted).Select(pi => new PortfolioItemDto
                {
                    Id = pi.Id,
                    Title = pi.Title,
                    Description = pi.Description,
                    Role = pi.Role,
                    VisitLink = pi.VisitLink,
                    ThumbnailUrl = pi.ThumbnailUrl,
                    Media = pi.Media.Select(m => new PortfolioMediaDto
                    {
                        Id = m.Id,
                        FileUrl = m.FileUrl,
                        FileType = m.FileType
                    }).ToList(),
                    CreatedAt = pi.CreatedAt
                }).ToList() ?? new List<PortfolioItemDto>(),

                Languages = freelancer?.Languages.Select(l => new LanguageDto
                {
                    Name = l.Name,
                    Level = l.Level
                }).ToList() ?? new List<LanguageDto>(),

                Education = freelancer?.Education.Select(e => new EducationDto
                {
                    School = e.School,
                    Degree = e.Degree,
                    FieldOfStudy = e.FieldOfStudy,
                    Year = e.DateEnd?.Year ?? e.DateStart.Year
                }).ToList() ?? new List<EducationDto>(),

                EmploymentHistory = workHistory
            };

            return new Result<PublicProfileDto>
            {
                Succeeded = true,
                Message = "Public profile retrieved successfully.",
                Data = publicProfile
            };
        }

        public async Task<Result<string>> UpdateFullNameAsync(string userId, string newName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<string>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to update full name.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            user.FullName = newName;
            await _userManager.UpdateAsync(user);

            return new Result<string>
            {
                Succeeded = true,
                Message   = "Full name updated successfully.",
                Data      = user.FullName
            };
        }

        public async Task<Result<string>> UpdateEmailAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<string>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to update email.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            var confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var emailSent         = await _emailService.SendConfirmationEmailAsync(userId, newEmail, confirmationToken);

            return emailSent
                ? new Result<string>
                {
                    Succeeded = true,
                    Message   = "Confirmation email sent to new address. Please confirm to complete the update.",
                    Data      = newEmail
                }
                : new Result<string>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailSendFailed,
                    Message   = "Failed to update email.",
                    Errors    = new List<string> { "Failed to send confirmation email." }
                };
        }

        public async Task<Result<string>> UpdateTitleAsync(string userId, string newTitle)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<string>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update title.",
                Data = null
            };

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<string>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update title.",
                Data = null
            };

            freelancer.Title = newTitle;
            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();

            return new Result<string>
            {
                Succeeded = true,
                Errors = { },
                Message = "Title updated successfully.",
                Data = freelancer.Title
            };
        }

        public async Task<Result<string?>> UpdateBioAsync(string userId, string? newBio)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<string?>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update bio.",
                Data = null
            };

            user.Bio = newBio;
            await _userManager.UpdateAsync(user);

            return new Result<string?>
            {
                Succeeded = true,
                Errors = { },
                Message = "Bio updated successfully.",
                Data = user.Bio
            };
        }

        public async Task<Result<ExperienceUpdateDto>> UpdateExperienceAsync(string userId, ExperienceUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<ExperienceUpdateDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update experience.",
                Data = null
            };

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<ExperienceUpdateDto>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update experience.",
                Data = null
            };

            if (!Enum.IsDefined(typeof(Entities.Enums.ExperienceLevel), dto.ExperienceLevel))
            {
                return new Result<ExperienceUpdateDto>
                {
                    Succeeded = false,
                    Errors = { "Invalid experience level value." },
                    Message = "Failed to update experience.",
                    Data = null
                };
            }

            freelancer.ExperienceLevel = (Entities.Enums.ExperienceLevel)dto.ExperienceLevel;
            freelancer.YearsOfExperience = dto.YearsOfExperience;

            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();

            return new Result<ExperienceUpdateDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Experience updated successfully.",
                Data = dto
            };
        }


        public async Task<Result<AccountUpdateDto>> UpdateAccountAsync(string userId, AccountUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<AccountUpdateDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to update account settings.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                await _userManager.SetEmailAsync(user, dto.Email);
                await _userManager.SetUserNameAsync(user, dto.Email);
            }

            if (dto.Bio != null)
            {
                user.Bio = dto.Bio;
            }

            await _userManager.UpdateAsync(user);

            return new Result<AccountUpdateDto>
            {
                Succeeded = true,
                Message   = "Account settings updated successfully.",
                Data      = dto
            };
        }

        public async Task<Result<LocationUpdateDto>> UpdateLocationAsync(string userId, LocationUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<LocationUpdateDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to update location settings.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            if (dto.Address       != null) user.Address       = dto.Address;
            if (dto.City          != null) user.City          = dto.City;
            if (dto.StateProvince != null) user.StateProvince = dto.StateProvince;
            if (dto.ZipCode       != null) user.ZipCode       = dto.ZipCode;
            if (dto.Country       != null) user.Country       = dto.Country;
            if (dto.TimeZone      != null) user.TimeZone      = dto.TimeZone;
            if (dto.PhoneNumber   != null) user.PhoneNumber   = dto.PhoneNumber;

            await _userManager.UpdateAsync(user);

            return new Result<LocationUpdateDto>
            {
                Succeeded = true,
                Message   = "Location settings updated successfully.",
                Data      = dto
            };
        }

        public async Task<Result<PaymentMethodReadDTO>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<PaymentMethodReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to add payment method.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            var paymentMethod = dto.ToPaymentMethod(userId);
            await _context.PaymentMethods.AddAsync(paymentMethod);
            await _context.SaveChangesAsync();

            return new Result<PaymentMethodReadDTO>
            {
                Succeeded = true,
                Message   = "Payment method added successfully.",
                Data      = paymentMethod.ToPaymentMethodRead()
            };
        }
        public async Task<Result<PaymentMethodReadDTO>> UpdateBillingAsync(string userId, string billingId, PaymentMethodUpdateDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<PaymentMethodReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message = "Failed to update payment method.",
                    Errors = new List<string> { "User not found." }
                };
            }
            var paymentMethod = await _context.PaymentMethods.FindAsync(billingId);
            if (paymentMethod == null || paymentMethod.UserId != userId)
            {
                return new Result<PaymentMethodReadDTO>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.PaymentMethodNotFound,
                    Message = "Failed to update payment method.",
                    Errors = new List<string> { "Payment method not found." }
                };
            }
            paymentMethod.AccountIdentifier = dto.AccountIdentifier;
            paymentMethod.Method = dto.Method;
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync();
            return new Result<PaymentMethodReadDTO>
            {
                Succeeded = true,
                Message = "Payment method updated successfully.",
                Data = paymentMethod.ToPaymentMethodRead()
            };
        }
        public async Task<Result<bool>> DeleteBillingAsync(string userId, string billingId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message = "Failed to delete payment method.",
                    Errors = new List<string> { "User not found." }
                };
            }

            var paymentMethod = await _context.PaymentMethods.FindAsync(billingId);
            if (paymentMethod == null || paymentMethod.UserId != userId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.PaymentMethodNotFound,
                    Message = "Failed to delete payment method.",
                    Errors = new List<string> { "Payment method not found." }
                };
            }

            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();

            return new Result<bool>
            {
                Succeeded = true,
                Message = "Payment method deleted successfully.",
                Data = true
            };
        }
        public async Task<Result<FreelancerReadDTO>> UpdateFreelancerDetailsAsync(string userId, FreelancerUpdateDTO updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<FreelancerReadDTO>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update freelancer details.",
                Data = null
            };

            // Call the shared freelancer update logic
            var success = await _freelancerService.UpdateFreelancerAsync(userId, updateDto);
            if (!success) return new Result<FreelancerReadDTO>
            {
                Succeeded = false,
                Errors = { "Failed to update freelancer profile." },
                Message = "Failed to update freelancer details.",
                Data = null
            };

            // Fetch the updated freelancer entity for the response
            var updatedUser = await _context.Users
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Languages)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.Education)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.ExperienceDetails)
                .Include(u => u.Freelancer)
                    .ThenInclude(f => f.EmploymentHistory)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return new Result<FreelancerReadDTO>
            {
                Succeeded = true,
                Message = "Freelancer details updated successfully.",
                Errors = { },
                Data = updatedUser.Freelancer_To_FreelancerRead()
            };
        }

        public async Task<Result<UserProfileDto>> GetFreelancerDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to retrieve freelancer details.",
                Data = null
            };

            var freelancer = await _context.Freelancers
                .Include(f => f.Languages)
                .Include(f => f.Education)
                .Include(f => f.ExperienceDetails)
                .Include(f => f.EmploymentHistory)
                .FirstOrDefaultAsync(f => f.UserId == userId);

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Message = "Freelancer details retrieved successfully.",
                Errors = { },
                Data = user.ToUserProfileDto(freelancer: freelancer)
            };
        }

    }
}
