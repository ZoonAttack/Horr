using System;

namespace Entities.Users.FreelancerHelpers
{
    public enum ExperienceCategory
    {
        Employment,
        Certification,
        Other
    }

    public class ProfessionalExperience
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ExperienceCategory Category { get; set; } // Requirement: Enum for Employment/Certification
        public bool IsDeleted { get; set; } // Requirement: IsDeleted flag for soft delete
    }
}
