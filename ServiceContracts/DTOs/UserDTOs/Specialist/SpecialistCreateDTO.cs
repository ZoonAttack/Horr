namespace Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for creating a new User and their initial Specialist profile.
    /// </summary>
    public class SpecialistCreateDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; } // The raw password to be hashed by the service layer
        public string Specialization { get; set; }
    }
}