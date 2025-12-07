using Entities.User; // Required to reference the Specialist and User entities

namespace Application.DTOs.User
{
    /// <summary>
    /// Static class containing mapping methods for Specialist profile DTOs.
    /// </summary>
    public static class SpecialistExtensions
    {
        /// <summary>
        /// Maps a User entity (which MUST have its SpecialistProfile loaded) to a SpecialistReadDTO.
        /// </summary>
        /// <param name="user">The User entity to map (must include the SpecialistProfile).</param>
        /// <returns>A SpecialistReadDTO or null if the input is null.</returns>
        public static SpecialistReadDTO User_To_SpecialistRead(this Entities.User.User user)
        {
            if (user == null)
            {
                return null;
            }

            return new SpecialistReadDTO
            {
                // User Mapping
                Id = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsVerified = user.IsVerified,
                TrustScore = user.TrustScore,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                // Specialist Profile Mapping (must check for existence)
                Specialization = user.SpecialistProfile?.Specialization
            };
        }

        /// <summary>
        /// Maps a SpecialistCreateDTO to a new User entity (The SpecialistProfile creation happens implicitly).
        /// </summary>
        /// <param name="createDto">The DTO containing creation data.</param>
        /// <returns>A new User entity ready for persistence.</returns>
        public static Entities.User.User SpecialistCreate_To_Specialist(this SpecialistCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            // Note: PasswordHash will be set by the service layer after hashing.
            // Role should be set to UserRole.Specialist by the service layer.
            return new Entities.User.User
            {
                FullName = createDto.FullName,
                Email = createDto.Email,
                Phone = createDto.Phone,

                // Initialize the SpecialistProfile navigation property with the specific data
                SpecialistProfile = new Specialist
                {
                    Specialization = createDto.Specialization
                    // UserId is set implicitly when User is saved
                }
            };
        }

        /// <summary>
        /// Applies updates from a SpecialistUpdateDTO to an existing User entity AND its related SpecialistProfile.
        /// </summary>
        /// <param name="user">The User entity to update (must include the SpecialistProfile).</param>
        /// <param name="updateDto">The DTO containing update data.</param>
        public static void SpecialistUpdate_To_Specialist(this Entities.User.User user, SpecialistUpdateDTO updateDto)
        {
            if (user == null || updateDto == null)
            {
                return;
            }

            // Apply updates to the User entity
            user.FullName = updateDto.FullName;
            user.Email = updateDto.Email;
            user.Phone = updateDto.Phone;

            // Apply updates to the Specialist Profile
            if (user.SpecialistProfile != null)
            {
                user.SpecialistProfile.Specialization = updateDto.Specialization;
            }
        }
    }
}