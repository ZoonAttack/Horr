using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;

namespace ServiceContracts.DTOs.Settings
{
    public class AccountUpdateDto
    {
        [MaxLength(150)]
        public string? FullName { get; set; }
        
        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }
    }

    public class LocationUpdateDto
    {
        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? StateProvince { get; set; }

        [MaxLength(20)]
        public string? ZipCode { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? TimeZone { get; set; }

        [RegularExpression(@"^(\+201)[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number format.")]
        public string? PhoneNumber { get; set; }
    }

    public class PrivacyResponseDto
    {
        public string UserIdHash { get; set; } = string.Empty;
        public Visibility Visibility { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
    }

    public class PrivacyUpdateDto
    {
        public Visibility? Visibility { get; set; }
        public ExperienceLevel? ExperienceLevel { get; set; }
    }
}
