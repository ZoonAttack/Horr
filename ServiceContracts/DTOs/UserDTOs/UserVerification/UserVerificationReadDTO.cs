using Entities.Enums;

namespace Services.DTOs.UserDTOs.UserVerification
{
    /// <summary>
    /// DTO for reading or displaying user verification information.
    /// </summary>
    public class UserVerificationReadDTO
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string NationalIdImage { get; set; }

        public string SelfieImage { get; set; }

        public VerificationStatus Status { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
