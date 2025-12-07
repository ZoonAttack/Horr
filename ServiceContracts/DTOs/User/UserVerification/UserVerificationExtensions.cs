using Entities.Enums;

namespace ServiceContracts.DTOs.User.UserVerification
{
    public static class UserVerificationExtensions
    {
        /// <summary>
        /// Converts UserVerification entity to UserVerificationReadDTO
        /// </summary>
        public static UserVerificationReadDTO UserVerification_To_UserVerificationRead(this Entities.User.UserVerification userVerification)
        {
            if (userVerification == null)
            {
                return null;
            }

            return new UserVerificationReadDTO
            {
                Id = userVerification.Id,
                UserId = userVerification.UserId,
                NationalIdImage = userVerification.NationalIdImage,
                SelfieImage = userVerification.SelfieImage,
                Status = userVerification.Status,
                ReviewedAt = userVerification.ReviewedAt,
                CreatedAt = userVerification.CreatedAt,
                UpdatedAt = userVerification.UpdatedAt
            };
        }

        /// <summary>
        /// Converts UserVerificationCreateDTO to UserVerification entity
        /// </summary>
        public static Entities.User.UserVerification UserVerificationCreate_To_UserVerification(this UserVerificationCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.User.UserVerification
            {
                UserId = createDto.UserId,
                NationalIdImage = createDto.NationalIdImage,
                SelfieImage = createDto.SelfieImage,
                Status = VerificationStatus.Pending
            };
        }

        /// <summary>
        /// Applies UserVerificationUpdateDTO to an existing UserVerification entity
        /// </summary>
        public static void UserVerificationUpdate_To_UserVerification(this Entities.User.UserVerification userVerification, UserVerificationUpdateDTO updateDto)
        {
            if (userVerification == null || updateDto == null)
            {
                return;
            }

            userVerification.Status = updateDto.Status;

            // Set ReviewedAt when status changes from Pending
            if (updateDto.Status != VerificationStatus.Pending && userVerification.ReviewedAt == null)
            {
                userVerification.ReviewedAt = DateTime.UtcNow;
            }
        }
    }
}
