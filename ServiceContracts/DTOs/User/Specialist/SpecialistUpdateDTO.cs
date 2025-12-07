using System;

namespace Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for updating an existing Specialist's combined profile
    /// </summary>
    public class SpecialistUpdateDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Specialization { get; set; }
    }
}