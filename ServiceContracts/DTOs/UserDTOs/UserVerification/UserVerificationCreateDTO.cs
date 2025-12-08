using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.UserDTOs.UserVerification
{
    /// <summary>
    /// DTO for creating a new UserVerification submission.
    /// </summary>
    public class UserVerificationCreateDTO
    {
        [Required]
        public string UserId { get; set; }

        [MaxLength(255)]
        public string NationalIdImage { get; set; }

        [MaxLength(255)]
        public string SelfieImage { get; set; }
    }
}
