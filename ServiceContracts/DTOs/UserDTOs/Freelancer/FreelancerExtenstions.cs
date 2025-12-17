using Mappers;

namespace ServiceContracts.DTOs.User.Freelancer
{
    public static class FreelancerExtensions
    {
        // =========================================================
        // 1. ENTITY TO READ DTO (Mapping from Entities.Users.User to FreelancerReadDTO)
        // =========================================================

        public static FreelancerReadDTO Freelancer_To_FreelancerRead(this Entities.Users.User user)
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
                Phone = user.PhoneNumber,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                // Freelancer Profile Mapping (must check for existence)
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
                PhoneNumber = createDto.Phone,

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
            user.FullName = updateDto.FullName;
            user.Email = updateDto.Email;
            user.PhoneNumber = updateDto.Phone;

            // Apply updates to the Freelancer Profile
            if (user.Freelancer != null)
            {
                user.Freelancer.HourlyRate = updateDto.HourlyRate;
                user.Freelancer.Availability = updateDto.Availability;
                user.Freelancer.YearsOfExperience = updateDto.YearsOfExperience;
                user.Freelancer.PortfolioUrl = updateDto.PortfolioUrl;

                // --- NEW MAPPING: PROFILE COLLECTIONS (Update Logic Handled by Service Layer) ---

                string freelancerId = user.Id;
            }
        }

        // Inside FreelancerExtensions static class
        public static FreelancerPublicReadDTO ToPublicReadDto(this Entities.Users.User user)
        {
            if (user == null) return null;

            var dto = new FreelancerPublicReadDTO
            {
                Id = user.Id,
                FullName = user.FullName,

                // Mapping the Public Trust Signals
                IsVerified = user.IsVerified,      
                TrustScore = user.TrustScore,       

                // Freelancer Profile Mapping (null-safe access)
                Bio = user.Freelancer?.Bio,
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
                    .Select(e => e.ToReadDto()).ToList() ?? new List<EmploymentReadDto>()
            };
            return dto;
        }
    }
}