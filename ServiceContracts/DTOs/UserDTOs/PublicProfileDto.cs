using ServiceContracts.DTOs.FreelancerProfile;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.UserDTOs
{
    public class PublicProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal TrustScore { get; set; }
        public bool IsVerified { get; set; }
        public int ExperienceLevel { get; set; }
        public List<string> Skills { get; set; } = new List<string>();          // skill names only
        public List<PortfolioItemDto> Portfolio { get; set; } = new List<PortfolioItemDto>();
    }
}
