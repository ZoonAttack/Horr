namespace ServiceContracts.DTOs.User
{
    public class LoginRequestDTO
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}