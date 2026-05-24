using System.Collections.Generic;

namespace ServiceContracts.DTOs.Recommendations
{
    public class RecommendedFreelancerDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string ExperienceLevel { get; set; } = string.Empty;
        public bool Availability { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
    }
}
