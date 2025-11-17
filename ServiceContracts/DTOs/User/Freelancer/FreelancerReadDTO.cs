using Entities;
using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.User.Freelancer
{
    /// <summary>
    /// DTO for reading/displaying Freelancer profile information,
    /// including linked User info as needed.
    /// </summary>
    public class FreelancerReadDTO
    {
        public long UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Bio { get; set; }

        public decimal? HourlyRate { get; set; }

        public string Availability { get; set; }

        public int? YearsOfExperience { get; set; }

        public string PortfolioUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
