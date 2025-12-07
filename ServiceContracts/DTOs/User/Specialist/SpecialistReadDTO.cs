using Entities.Enums;

namespace Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for reading combined Specialist and User profile information.
    /// </summary>
    public class SpecialistReadDTO
    {
        public long Id { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Specialization { get; set; }
    }
}