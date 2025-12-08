using System;
using Entities.Users;
using Entities.Enums;

namespace ServiceContracts.DTOs.User.Freelancer
{
    public class FreelancerCreateDTO
    {
        // --- Core User Properties (Write) ---
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

        // --- Freelancer Profile Properties ---
        public string Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }
    }
}