using System;

namespace ServiceContracts.DTOs.User.Client
{
    public class ClientUpdateDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }
    }
}