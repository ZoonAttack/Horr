using Mappers;
using ServiceContracts.DTOs.Skill.FreelancerSkill;

namespace ServiceContracts.DTOs.UserDTOs.FreelancerManagement
{
    public static class FreelancerExtensions
    {
        // =========================================================
        // 1. ENTITY TO READ DTO (Mapping from Entities.Users.User to FreelancerReadDTO)
        // =========================================================

        public static FreelancerReadDTO Freelancer_To_FreelancerRead(this Entities.Users.User user)
        {
            return Freelancer_To_FreelancerRead(user, false, 0.0, 0);
        }

        public static FreelancerReadDTO Freelancer_To_FreelancerRead(
            this Entities.Users.User user, 
            bool isSaved, 
            double averageRating, 
            int totalReviews)
        {
            if (user == null)
            {
                return null;
            }

            var dto = new FreelancerReadDTO
            {
                // User Mapping
                Id = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicturePath = user.ProfilePicturePath,
                IsVerified = user.IsVerified,
                TrustScore = user.TrustScore,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                IsSaved = isSaved,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                Address = user.Address,
                City = user.City,
                StateProvince = user.StateProvince,
                ZipCode = user.ZipCode,
                Country = user.Country,
                TimeZone = user.TimeZone,

                // Freelancer Profile Mapping (must check for existence)
                Title = user.Freelancer?.Title,
                Bio = user.Bio,
                HourlyRate = user.Freelancer?.HourlyRate,
                Availability = user.Freelancer?.Availability,
                YearsOfExperience = user.Freelancer?.YearsOfExperience,
                PortfolioUrl = user.Freelancer?.PortfolioUrl
            };

            // --- NEW MAPPING: PROFILE COLLECTIONS (Using Helper Extensions) ---
            if (user.Freelancer != null)
            {
                // Languages
                dto.Languages = user.Freelancer.Languages?
                    .Select(l => l.ToReadDto()).ToList() ?? new List<LanguageReadDto>();

                // Education
                dto.Education = user.Freelancer.Education?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<EducationReadDto>();

                // Experience Details
                dto.ExperienceDetails = user.Freelancer.ExperienceDetails?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<ExperienceDetailReadDto>();

                // Employment History
                dto.EmploymentHistory = user.Freelancer.EmploymentHistory?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<EmploymentReadDto>();

                // Skills
                dto.Skills = user.Freelancer.FreelancerSkills?
                    .Select(fs => fs.FreelancerSkill_To_FreelancerSkillRead()).ToList() ?? new List<FreelancerSkillReadDTO>();
            }

            return dto;
        }

        public static FreelancerSearchResultDTO Freelancer_To_FreelancerSearchResult(this Entities.Users.User user)
        {
            return Freelancer_To_FreelancerSearchResult(user, false, 0.0, 0);
        }

        public static FreelancerSearchResultDTO Freelancer_To_FreelancerSearchResult(
            this Entities.Users.User user, 
            bool isSaved, 
            double averageRating, 
            int totalReviews)
        {
            if (user == null)
            {
                return null;
            }

            var dto = new FreelancerSearchResultDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Title = user.Freelancer?.Title,
                ProfilePicturePath = user.ProfilePicturePath,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                HourlyRate = user.Freelancer?.HourlyRate,
                TrustScore = user.TrustScore,
                Availability = user.Freelancer?.Availability ?? string.Empty,
                Bio = user.Bio,
                IsSaved = isSaved
            };

            if (user.Freelancer != null)
            {
                dto.Skills = user.Freelancer.FreelancerSkills?
                    .Select(fs => fs.FreelancerSkill_To_FreelancerSkillRead()).ToList() ?? new List<FreelancerSkillReadDTO>();
            }

            return dto;
        }

        // =========================================================
        // 2. CREATE DTO TO ENTITY (Mapping from FreelancerCreateDTO to Entities.Users.User)
        // =========================================================

        public static Entities.Users.User FreelancerCreate_To_User(this FreelancerCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            var userEntity = new Entities.Users.User
            {
                FullName = createDto.FullName,
                Email = createDto.Email,
                UserName = createDto.Email, // Identity requires UserName
                PhoneNumber = createDto.Phone,
                Bio = createDto.Bio,
                Address = createDto.Address ?? string.Empty,
                City = createDto.City ?? string.Empty,
                StateProvince = createDto.StateProvince ?? string.Empty,
                ZipCode = createDto.ZipCode ?? string.Empty,
                Country = createDto.Country ?? "Egypt",
                TimeZone = createDto.TimeZone ?? "UTC+02:00",

                // --- NEW FREELANCER PROFILE CREATION ---
                Freelancer = new Entities.Users.Freelancer
                {
                    HourlyRate = createDto.HourlyRate,
                    Availability = createDto.Availability,
                    YearsOfExperience = createDto.YearsOfExperience,
                    PortfolioUrl = createDto.PortfolioUrl
                }
            };

            if (userEntity.Freelancer != null)
            {
                // We pass a placeholder string as the final ID isn't known yet.
                string placeholderId = Guid.NewGuid().ToString();

                userEntity.Freelancer.Languages = createDto.Languages
                    .Select(l => l.ToEntity(placeholderId)).ToList();

                userEntity.Freelancer.Education = createDto.Education
                    .Select(e => e.ToEntity(placeholderId)).ToList();

                userEntity.Freelancer.ExperienceDetails = createDto.ExperienceDetails
                    .Select(e => e.ToEntity(placeholderId)).ToList();

                userEntity.Freelancer.EmploymentHistory = createDto.EmploymentHistory
                    .Select(e => e.ToEntity(placeholderId)).ToList();
            }

            return userEntity;
        }

        // =========================================================
        // 3. UPDATE DTO TO ENTITY (Mapping from FreelancerUpdateDTO to Entities.Users.User)
        // =========================================================

        public static void FreelancerUpdate_To_Freelancer(this Entities.Users.User user, FreelancerUpdateDTO updateDto)
        {
            if (user == null || updateDto == null)
            {
                return;
            }

            // Apply updates to the User entity
            if (!string.IsNullOrEmpty(updateDto.FullName)) user.FullName = updateDto.FullName;
            if (!string.IsNullOrEmpty(updateDto.Email)) 
            {
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
            }
            user.PhoneNumber = updateDto.PhoneNumber;
            user.Bio = updateDto.Bio;
            user.Address = updateDto.Address;
            user.City = updateDto.City;
            user.StateProvince = updateDto.StateProvince;
            user.ZipCode = updateDto.ZipCode;
            user.Country = updateDto.Country;
            user.TimeZone = updateDto.TimeZone;

            // Apply updates to the Freelancer Profile
            if (user.Freelancer != null)
            {
                if (!string.IsNullOrEmpty(updateDto.Title)) user.Freelancer.Title = updateDto.Title;
                if (updateDto.HourlyRate.HasValue) user.Freelancer.HourlyRate = updateDto.HourlyRate;
                if (!string.IsNullOrEmpty(updateDto.Availability)) user.Freelancer.Availability = updateDto.Availability;
                if (updateDto.YearsOfExperience.HasValue) user.Freelancer.YearsOfExperience = updateDto.YearsOfExperience;
                if (!string.IsNullOrEmpty(updateDto.PortfolioUrl)) user.Freelancer.PortfolioUrl = updateDto.PortfolioUrl;

                if (updateDto.ExperienceLevel.HasValue)
                {
                    user.Freelancer.ExperienceLevel = (Entities.Enums.ExperienceLevel)updateDto.ExperienceLevel.Value;
                }

                // --- NEW MAPPING: PROFILE COLLECTIONS (Update Logic Handled by Service Layer) ---
            }
        }

        // Inside FreelancerExtensions static class
        public static FreelancerPublicReadDTO ToPublicReadDto(this Entities.Users.User user)
        {
            return ToPublicReadDto(user, 0.0, 0);
        }

        public static FreelancerPublicReadDTO ToPublicReadDto(
            this Entities.Users.User user,
            double averageRating,
            int totalReviews)
        {
            if (user == null) return null;

            var dto = new FreelancerPublicReadDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                ProfilePicturePath = user.ProfilePicturePath,

                // Mapping the Public Trust Signals
                IsVerified = user.IsVerified,      
                TrustScore = user.TrustScore,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                City = user.City,
                Country = user.Country,

                // Freelancer Profile Mapping (null-safe access)
                Title = user.Freelancer?.Title,
                Bio = user.Bio,
                HourlyRate = user.Freelancer?.HourlyRate,
                Availability = user.Freelancer?.Availability,
                YearsOfExperience = user.Freelancer?.YearsOfExperience,
                PortfolioUrl = user.Freelancer?.PortfolioUrl,

                // Collections (using existing helper methods)
                Languages = user.Freelancer?.Languages?
                    .Select(l => l.ToReadDto()).ToList() ?? new List<LanguageReadDto>(),
                Education = user.Freelancer?.Education?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<EducationReadDto>(),
                ExperienceDetails = user.Freelancer?.ExperienceDetails?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<ExperienceDetailReadDto>(),
                EmploymentHistory = user.Freelancer?.EmploymentHistory?
                    .Select(e => e.ToReadDto()).ToList() ?? new List<EmploymentReadDto>(),
                Skills = user.Freelancer?.FreelancerSkills?
                    .Select(fs => fs.FreelancerSkill_To_FreelancerSkillRead()).ToList() ?? new List<FreelancerSkillReadDTO>()
            };
            return dto;
        }
    }
}
