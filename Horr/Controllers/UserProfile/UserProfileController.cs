using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.Settings;

namespace Horr.Controllers.UserProfile
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileSettings _profileSettingsService;

        public UserProfileController(IProfileSettings profileSettingsService)
        {
            _profileSettingsService = profileSettingsService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.GetProfileAsync(userId);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(response);
        }

        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] string fullname)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);//This is for MVC. For API, another validation is required
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateFullNameAsync(userId, fullname);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Name updated successfully.",  data = response });
        }

        [HttpPatch("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] string email)
        {
            //Feels like the name is misleading.. gotta change it later!
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);

            var response = await _profileSettingsService.UpdateEmailAsync(userId, email);
            //Changing the actual email is an Authentication concern. so this service only sends the confirmation email
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = response.Message, data = response });
        }

        [HttpPost("payment-method")]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethodCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.CreateBillingAsync(userId, dto);
            if (!response.Succeeded) return NotFound("User not found.");
            return Ok(new { message = "Billing information created successfully." });
        }

        //[HttpPatch("account")]
        //public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateDto dto)
        //{
        //    if (!ModelState.IsValid) return BadRequest(ModelState);

        //    var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
        //    //var success = await _settingsService.UpdateAccountAsync(userId, dto);

        //    //if (!success) return NotFound("User not found.");

        //    return Ok(new { message = "Account updated successfully." });
        //}

        [HttpPatch("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateLocationAsync(userId, dto);

            if (!response.Succeeded) return NotFound("User not found.");

            return Ok(new { message = "Location updated successfully." });
        }

        [HttpGet("privacy")]
        public async Task<IActionResult> GetPrivacy()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.GetPrivacySettingsAsync(userId);

            if (response.Data == null) return NotFound("Freelancer profile not found for user.");

            return Ok(response);
        }

        [HttpPatch("privacy")]
        public async Task<IActionResult> UpdatePrivacy([FromBody] PrivacyUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdatePrivacySettingsAsync(userId, dto);

            if (!response.Succeeded) return NotFound("Freelancer profile not found.");

            return Ok(new { message = "Privacy settings updated successfully." });
        }
    }
}
