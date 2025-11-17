using ServiceContracts.DTOs.User.Freelancer;

namespace ServiceContracts.DTOs.User.Freelancer
{
    public static class FreelancerExtensions
    {
        /// <summary>
        /// Covnerts from Freelancer to FreelancerRead
        /// </summary>
        /// <param name="freelancer"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static FreelancerReadDTO Freelancer_To_FreelancerRead(this Entities.User.Freelancer freelancer, Entities.User.User user)
        {
            return new FreelancerReadDTO
            {
                UserId = freelancer.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Bio = freelancer.Bio,
                HourlyRate = freelancer.HourlyRate,
                Availability = freelancer.Availability,
                YearsOfExperience = freelancer.YearsOfExperience,
                PortfolioUrl = freelancer.PortfolioUrl,
                CreatedAt = freelancer.CreatedAt,
                UpdatedAt = freelancer.UpdatedAt
            };
        }

        /// <summary>
        /// Covnerts from FreelancerCreate to Freelancer
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Entities.User.Freelancer FreelancerCreate_To_Freelancer(this FreelancerCreateDTO dto, long userId)
        {
            return new Entities.User.Freelancer
            {
                UserId = userId,
                Bio = dto.Bio,
                HourlyRate = dto.HourlyRate,
                Availability = dto.Availability,
                YearsOfExperience = dto.YearsOfExperience,
                PortfolioUrl = dto.PortfolioUrl
            };
        }

        /// <summary>
        /// Apply updates for freelancer entity
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="freelancer"></param>
        public static void FreelancerUpdate_To_Freelancer(this FreelancerUpdateDTO dto, Entities.User.Freelancer freelancer)
        {
            if (dto.Bio != null)
                freelancer.Bio = dto.Bio;
            if (dto.HourlyRate.HasValue)
                freelancer.HourlyRate = dto.HourlyRate;
            if (!string.IsNullOrEmpty(dto.Availability))
                freelancer.Availability = dto.Availability;
            if (dto.YearsOfExperience.HasValue)
                freelancer.YearsOfExperience = dto.YearsOfExperience;
            if (!string.IsNullOrEmpty(dto.PortfolioUrl))
                freelancer.PortfolioUrl = dto.PortfolioUrl;
        }
    }
}