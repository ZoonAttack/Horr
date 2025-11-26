using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.User.UserVerification
{
    /// <summary>
    /// DTO for creating a new UserVerification submission.
    /// </summary>
    public class UserVerificationCreateDTO
    {
        [Required]
        public long UserId { get; set; }

        [MaxLength(255)]
        public string NationalIdImage { get; set; }

        [MaxLength(255)]
        public string SelfieImage { get; set; }
    }
}
