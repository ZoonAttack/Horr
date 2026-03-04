using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.Settings;

namespace Horr.Controllers.UserProfile
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdVal, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid User ID");
        }

        [HttpPatch("account")]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var success = await _settingsService.UpdateAccountAsync(userId, dto);
            
            if (!success) return NotFound("User not found.");
            
            return Ok(new { message = "Account updated successfully." });
        }

        [HttpPatch("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var success = await _settingsService.UpdateLocationAsync(userId, dto);
            
            if (!success) return NotFound("User not found.");

            return Ok(new { message = "Location updated successfully." });
        }

        [HttpGet("privacy")]
        public async Task<IActionResult> GetPrivacy()
        {
            var userId = GetCurrentUserId();
            var privacy = await _settingsService.GetPrivacySettingsAsync(userId);
            
            if (privacy == null) return NotFound("Freelancer profile not found for user.");

            return Ok(privacy);
        }

        [HttpPatch("privacy")]
        public async Task<IActionResult> UpdatePrivacy([FromBody] PrivacyUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var success = await _settingsService.UpdatePrivacySettingsAsync(userId, dto);
            
            if (!success) return NotFound("Freelancer profile not found.");

            return Ok(new { message = "Privacy settings updated successfully." });
        }
    }
}
