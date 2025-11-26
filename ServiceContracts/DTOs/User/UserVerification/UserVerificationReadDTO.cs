using Entities.Enums;

namespace ServiceContracts.DTOs.User.UserVerification
{
    /// <summary>
    /// DTO for reading or displaying user verification information.
    /// </summary>
    public class UserVerificationReadDTO
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string NationalIdImage { get; set; }

        public string SelfieImage { get; set; }

        public VerificationStatus Status { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
