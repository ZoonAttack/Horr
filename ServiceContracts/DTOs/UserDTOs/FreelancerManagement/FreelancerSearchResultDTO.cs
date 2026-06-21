using System.Collections.Generic;
using ServiceContracts.DTOs.Skill.FreelancerSkill;

namespace ServiceContracts.DTOs.UserDTOs.FreelancerManagement
{
    public class FreelancerSearchResultDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ProfilePicturePath { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int JobSuccessPercentage { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? OriginalCurrency { get; set; }
        public decimal? ConvertedHourlyRate { get; set; }
        public string? ConvertedCurrency { get; set; }
        public decimal TrustScore { get; set; }
        public string Availability { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsSaved { get; set; }
        public ICollection<FreelancerSkillReadDTO> Skills { get; set; } = new List<FreelancerSkillReadDTO>();
    }
}
