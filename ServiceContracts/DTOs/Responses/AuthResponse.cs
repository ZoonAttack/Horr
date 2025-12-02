using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Responses
{
    public class AuthResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }        // The JWT Access Token
        public string RefreshToken { get; set; } // The Refresh Token
        public DateTime ExpiresAt { get; set; }  // When the token dies
    }
}
