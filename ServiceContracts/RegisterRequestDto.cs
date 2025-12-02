using Entities.Enums;

namespace ServiceContracts
{
    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public UserRole Role { get; set; }
    }
}