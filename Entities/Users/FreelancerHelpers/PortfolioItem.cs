using System;

namespace Entities.Users.FreelancerHelpers
{
    public class PortfolioItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty; // Requirement: MediaUrl for "View Media"
        public bool IsDeleted { get; set; }
    }
}
