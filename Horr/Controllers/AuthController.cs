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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IAuthService authService, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _authService = authService;
            _userManager = userManager;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            // React will receive: { succeeded: true, data: { accessToken: "...", ... } }
            return Ok(result);
        }

        #region Helper
        private async Task<IActionResult> RedirectToRoleDashboard(User user)
        {
            // Get the list of roles for this specific user
            var roles = await _userManager.GetRolesAsync(user);

            // Check roles and redirect accordingly
            if (roles.Contains(UserRole.Freelancer.ToString()))
            {
                return RedirectToAction("Index", "FreelancerController");
            }
            else if (roles.Contains(UserRole.Client.ToString()))
            {
                return RedirectToAction("Index", "ClientDashboard");
            }

            // Default fallback (e.g., Home Page) if they have no role
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}