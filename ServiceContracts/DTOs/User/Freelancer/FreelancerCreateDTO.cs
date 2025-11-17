using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.User.Freelancer
{
    /// <summary>
    /// DTO for creating a new Freelancer profile linked to a User.
    /// Contains Freelancer-specific properties only.
    /// </summary>
    public class FreelancerCreateDTO
    {
        [MaxLength(1000)]
        public string Bio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? HourlyRate { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; }

        public int? YearsOfExperience { get; set; }

        [MaxLength(255)]
        [Url]
        public string PortfolioUrl { get; set; }
    }
}
