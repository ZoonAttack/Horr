using Entities.Enums;

namespace ServiceContracts.DTOs.User.UserVerification
{
    /// <summary>
    /// DTO for updating UserVerification status (typically by admin/specialist).
    /// </summary>
    public class UserVerificationUpdateDTO
    {
        public VerificationStatus Status { get; set; }
    }
}
