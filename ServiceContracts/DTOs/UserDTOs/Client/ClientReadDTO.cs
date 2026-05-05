using System;
using Entities.Enums;

namespace Services.DTOs.UserDTOs.Client
{
    public class ClientReadDTO
    {
        public string Id { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Bio { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TimeZone { get; set; }
    }
}
