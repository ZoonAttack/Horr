using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.User.Freelancer
{
    /// <summary>
    /// DTO for updating an existing Freelancer profile.
    /// </summary>
    public class FreelancerUpdateDTO
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
