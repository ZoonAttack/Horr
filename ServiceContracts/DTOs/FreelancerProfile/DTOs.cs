using System;

namespace ServiceContracts.DTOs.FreelancerProfile
{
    public class PortfolioCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
    }

    public class PortfolioResponseDto
    {
        public string Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
    }

    public class ExperienceResponseDto
    {
        public string Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
