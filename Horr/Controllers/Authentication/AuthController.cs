using Entities.Enums;
using Entities.Users;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Services.Authentication;
using Services.DTOs.Authentication;
using Services.DTOs.UserDTOs;
using System.Text;
namespace Horr.Controllers.Authentication
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


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (userId == null)
                return Unauthorized(new { Message = "User ID claim is missing." });
            var result = await _authService.ChangePasswordAsync(userId, dto);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result); // Password changed successfully
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

        [HttpPatch("change-email")]
        public async Task<IActionResult> ChangeEmail(string userId,string newEmail, string token)
        {
            var user = await _signInManager.UserManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return Unauthorized(new { Message = "User not found." });

            var result = await _authService.ChangeEmailAsync(user.Id, newEmail, token);

            if(!result.Succeeded)
                return BadRequest(result);
            return Ok(result); 
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
            SetRefreshTokenInCookie(result.Data.RefreshToken);
            return Ok(result.Data.Token);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken");
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(new { Message = "Refresh token is missing." });
            }
            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Succeeded)
                return Unauthorized(result);

            SetRefreshTokenInCookie(result.Data.RefreshToken);

            return Ok(result.Data.Token);
        }


        #region helper
        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7), // Set expiration as needed
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        #endregion
    }
}
