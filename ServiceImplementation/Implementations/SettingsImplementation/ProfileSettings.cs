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

namespace ServiceImplementation.Implementations.Settings
{
    public class ProfileSettings : IProfileSettings
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public ProfileSettings(UserManager<User> userManager, IEmailService emailService, AppDbContext context)
        {
            _userManager  = userManager;
            _emailService = emailService;
            _context      = context;
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

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Profile retrieved successfully.",
                Data = user.ToUserProfileDto(freelancer: freelancer)
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

            var publicProfile = new PublicProfileDto
            {
                FullName = user.FullName,
                Title = freelancer?.Title,
                Bio = freelancer?.Bio,
                City = user.City,
                Country = user.Country,
                ProfilePicturePath = user.ProfilePicturePath,
                TrustScore = user.TrustScore,
                IsVerified = user.IsVerified,
                ExperienceLevel = (int)(freelancer?.ExperienceLevel ?? Entities.Enums.ExperienceLevel.Beginner),
                YearsOfExperience = freelancer?.YearsOfExperience,

                TotalEarnings = "$0", 
                TotalJobs = 0,
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

                EmploymentHistory = freelancer?.EmploymentHistory.Select(eh => new EmploymentDto
                {
                    Company = eh.Company,
                    Title = eh.Title,
                    DateRange = $"{eh.FromDate:MMM yyyy} - {(eh.CurrentlyWorkThere ? "Present" : eh.ToDate?.ToString("MMM yyyy"))}",
                    Description = "" 
                }).ToList() ?? new List<EmploymentDto>()
            };

            return new Result<PublicProfileDto>
            {
                Succeeded = true,
                Message = "Public profile retrieved successfully.",
                Data = publicProfile
            };
        }

        public async Task<Result<UserProfileDto>> UpdateFullNameAsync(string userId, string newName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<UserProfileDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to update full name.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            user.FullName = newName;
            await _userManager.UpdateAsync(user);

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Message   = "Full name updated successfully.",
                Data      = user.ToUserProfileDto()
            };
        }

        public async Task<Result<UserProfileDto>> UpdateEmailAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<UserProfileDto>
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
                ? new Result<UserProfileDto>
                {
                    Succeeded = true,
                    Message   = "Confirmation email sent to new address. Please confirm to complete the update.",
                    Data      = user.ToUserProfileDto(pendingEmail: newEmail)
                }
                : new Result<UserProfileDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.EmailSendFailed,
                    Message   = "Failed to update email.",
                    Errors    = new List<string> { "Failed to send confirmation email." }
                };
        }

        public async Task<Result<UserProfileDto>> UpdateTitleAsync(string userId, string newTitle)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update title.",
                Data = null
            };

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update title.",
                Data = null
            };

            freelancer.Title = newTitle;
            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Title updated successfully.",
                Data = user.ToUserProfileDto(freelancer: freelancer)
            };
        }

        public async Task<Result<UserProfileDto>> UpdateBioAsync(string userId, string newBio)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update bio.",
                Data = null
            };

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update bio.",
                Data = null
            };

            freelancer.Bio = newBio;
            _context.Freelancers.Update(freelancer);
            await _context.SaveChangesAsync();

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Bio updated successfully.",
                Data = user.ToUserProfileDto(freelancer: freelancer)
            };
        }

        public async Task<Result<UserProfileDto>> UpdateExperienceAsync(string userId, ExperienceUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "User not found." },
                Message = "Failed to update experience.",
                Data = null
            };

            var freelancer = await _context.Freelancers.FirstOrDefaultAsync(f => f.UserId == userId);
            if (freelancer == null) return new Result<UserProfileDto>
            {
                Succeeded = false,
                Errors = { "Freelancer profile not found." },
                Message = "Failed to update experience.",
                Data = null
            };

            if (!Enum.IsDefined(typeof(Entities.Enums.ExperienceLevel), dto.ExperienceLevel))
            {
                return new Result<UserProfileDto>
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

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Errors = { },
                Message = "Experience updated successfully.",
                Data = user.ToUserProfileDto(freelancer: freelancer)
            };
        }


        public async Task<Result<UserProfileDto>> UpdateAccountAsync(string userId, AccountUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<UserProfileDto>
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
                user.Bio = dto.Bio;

            await _userManager.UpdateAsync(user);

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Message   = "Account settings updated successfully.",
                Data      = user.ToUserProfileDto()
            };
        }

        public async Task<Result<UserProfileDto>> UpdateLocationAsync(string userId, LocationUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<UserProfileDto>
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

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Message   = "Location settings updated successfully.",
                Data      = user.ToUserProfileDto()
            };
        }

        public async Task<Result<UserProfileDto>> CreateBillingAsync(string userId, PaymentMethodCreateDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new Result<UserProfileDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.UserNotFound,
                    Message   = "Failed to add payment method.",
                    Errors    = new List<string> { "User not found." }
                };
            }

            await _context.PaymentMethods.AddAsync(dto.ToPaymentMethod(userId));
            await _context.SaveChangesAsync();

            return new Result<UserProfileDto>
            {
                Succeeded = true,
                Message   = "Payment method added successfully.",
                Data      = user.ToUserProfileDto()
            };
        }
    }
}
