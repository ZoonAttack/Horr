namespace ServiceContracts.DTOs.User.Freelancer
{
    public static class FreelancerExtensions
    {
        public static FreelancerReadDTO Freelancer_To_FreelancerRead(this Entities.Users.User user)
        {
            if (user == null)
            {
                return null;
            }

            return new FreelancerReadDTO
            {
                // User Mapping
                Id = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsVerified = user.IsVerified,
                TrustScore = user.TrustScore,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                // Freelancer Profile Mapping (must check for existence)
                Bio = user.Freelancer?.Bio,
                HourlyRate = user.Freelancer?.HourlyRate,
                Availability = user.Freelancer?.Availability,
                YearsOfExperience = user.Freelancer?.YearsOfExperience,
                PortfolioUrl = user.Freelancer?.PortfolioUrl
            };
        }

        public static Entities.Users.User FreelancerCreate_To_User(this FreelancerCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Users.User
            {
                FullName = createDto.FullName,
                Email = createDto.Email,
                PhoneNumber = createDto.Phone,

                Freelancer = new Entities.Users.Freelancer
                {
                    Bio = createDto.Bio,
                    HourlyRate = createDto.HourlyRate,
                    Availability = createDto.Availability,
                    YearsOfExperience = createDto.YearsOfExperience,
                    PortfolioUrl = createDto.PortfolioUrl
                }
            };
        }

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
                user.Freelancer.Bio = updateDto.Bio;
                user.Freelancer.HourlyRate = updateDto.HourlyRate;
                user.Freelancer.Availability = updateDto.Availability;
                user.Freelancer.YearsOfExperience = updateDto.YearsOfExperience;
                user.Freelancer.PortfolioUrl = updateDto.PortfolioUrl;
            }
        }
    }
}