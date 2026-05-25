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

        [HttpGet("freelancer-details")]
        public async Task<IActionResult> GetFreelancerDetails()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _profileSettingsService.GetFreelancerDetailsAsync(userId);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(result.Data);
        }

        [HttpGet("public/{userIdHash}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(string userIdHash)
        {
            var response = await _profileSettingsService.GetPublicProfileAsync(userIdHash);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(response);
        }

        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] string fullname)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateFullNameAsync(userId, fullname);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Name updated successfully.",  data = response.Data });
        }

        [HttpPatch("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);

            var response = await _profileSettingsService.UpdateEmailAsync(userId, email);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = response.Message, data = response.Data });
        }

        [HttpPatch("title")]
        public async Task<IActionResult> UpdateTitle([FromBody] string title)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateTitleAsync(userId, title);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Title updated successfully.", data = response.Data });
        }

        [HttpPatch("bio")]
        public async Task<IActionResult> UpdateBio([FromBody] string? bio)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateBioAsync(userId, bio);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Bio updated successfully.", data = response.Data });
        }

        [HttpPatch("experience-level")]
        public async Task<IActionResult> UpdateExperienceLevel([FromBody] ExperienceUpdateDto dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateExperienceAsync(userId, dto);
            if (!response.Succeeded) return BadRequest(response.Errors);
            return Ok(new { message = "Experience updated successfully.", data = response.Data });
        }

        [HttpPost("payment-method")]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethodCreateDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.CreateBillingAsync(userId, dto);
            if (!response.Succeeded) return NotFound("User not found.");
            return Ok(new { message = "Billing information created successfully.", data = response.Data });
        }
        [HttpPatch("payment-method/{id}")]
        public async Task<IActionResult> UpdatePaymentMethod([FromRoute] string id, [FromBody] PaymentMethodUpdateDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateBillingAsync(userId, id, dto);
            if (!response.Succeeded) return NotFound("User not found.");
            return Ok(response.Data);
        }

        [HttpDelete("payment-method/{id}")]
        public async Task<IActionResult> DeletePaymentMethod([FromRoute] string id)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.DeleteBillingAsync(userId, id);
            if (!response.Succeeded) return NotFound("User not found.");
            return NoContent();
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

        [HttpPatch("freelancer-details")]
        public async Task<IActionResult> UpdateFreelancerDetails([FromBody] ServiceContracts.DTOs.UserDTOs.FreelancerManagement.FreelancerUpdateDTO dto)
        {
            if (!ModelState.IsValid) 
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { errors });
            }

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateFreelancerDetailsAsync(userId, dto);

            if (!response.Succeeded) 
            {
                return BadRequest(new { errors = response.Errors, message = response.Message });
            }

            return Ok(new { message = "Freelancer details updated successfully.", data = response.Data });
        }

    }
}
