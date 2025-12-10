using Entities.Enums;
using Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceImplementation.Authentication;
using Services.DTOs.UserDTOs;
namespace Horr.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IAuthService authService, SignInManager<User> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            // 1. Service Call
            var result = await _authService.RegisterAsync(dto);

            // 2. Handle Failure
            if (!result.Succeeded)
            {
                return BadRequest(result); // Returns 400 with error messages
            }
            //Email confirmation is handled in the service
            return Ok(result);// Meaning the email was sent successfully
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // Token handling happens in the service
            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result); // Email confirmed successfully(should redirect to login page in react for now)
        }
        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            var result = await _authService.ResendConfirmationEmailAsync(email);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result); // Confirmation email resent successfully
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            // React will receive: { succeeded: true, data: { accessToken: "...", ... } }
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if(_signInManager.IsSignedIn(User) == false)
            {
                return BadRequest(new { Message = "No user is currently logged in." });
            }
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}