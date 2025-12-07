using System;
using Entities.User;
using Entities.Enums;

namespace ServiceContracts.DTOs.User.Client
{
    public class ClientCreateDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
    }
}