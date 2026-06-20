using System;

namespace ServiceContracts.DTOs.UserDTOs
{
    public class FreelancerReviewDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ProjectTitle { get; set; }
    }
}
