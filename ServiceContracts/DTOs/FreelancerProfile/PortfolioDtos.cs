using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.FreelancerProfile
{
    public class PortfolioItemDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Role { get; set; }
        public string? VisitLink { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<PortfolioMediaDto> Media { get; set; } = new List<PortfolioMediaDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class PortfolioMediaDto
    {
        public string Id { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }  // "Image" | "Video"
    }
}
