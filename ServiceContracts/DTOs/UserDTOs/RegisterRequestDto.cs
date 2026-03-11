using Entities.Enums;

namespace Services.DTOs.UserDTOs
{
    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public UserRole Role { get; set; }

        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? TimeZone { get; set; }
        public string? ZipCode { get; set; }
    }
}
