using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.Settings;

namespace Horr.Controllers.UserProfile
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileSettings _settingsService;

        public UserProfileController(IProfileSettings settingsService)
        {
            _settingsService = settingsService;
        }
        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] string fullname)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var success = await _settingsService.UpdateFullNameAsync(userId, fullname);
            //if (!success) return NotFound("User not found.");
            return Ok(new { message = "Name updated successfully.",  data = success });
        }
        [HttpPatch("account")]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            //var success = await _settingsService.UpdateAccountAsync(userId, dto);

            //if (!success) return NotFound("User not found.");

            return Ok(new { message = "Account updated successfully." });
        }

        [HttpPatch("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var success = await _settingsService.UpdateLocationAsync(userId, dto);

            if (!success) return NotFound("User not found.");

            return Ok(new { message = "Location updated successfully." });
        }

        [HttpGet("privacy")]
        public async Task<IActionResult> GetPrivacy()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var privacy = await _settingsService.GetPrivacySettingsAsync(userId);

            if (privacy == null) return NotFound("Freelancer profile not found for user.");

            return Ok(privacy);
        }

        [HttpPatch("privacy")]
        public async Task<IActionResult> UpdatePrivacy([FromBody] PrivacyUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var success = await _settingsService.UpdatePrivacySettingsAsync(userId, dto);

            if (!success) return NotFound("Freelancer profile not found.");

            return Ok(new { message = "Privacy settings updated successfully." });
        }

        private string getCurrentUserId()
        {
            var userIdClaim = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier) : throw ;
            if (userIdClaim == null)
                throw new Exception("User ID claim not found. User might not be authenticated.");
            return userIdClaim?.Value;
        }
    }
}
