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

        /// <summary>
        /// Retrieves the profile details of the logged-in user.
        /// </summary>
        /// <returns>The user profile details.</returns>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.GetProfileAsync(userId);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves freelancer-specific profile details for the logged-in user.
        /// </summary>
        /// <returns>The freelancer profile details.</returns>
        [HttpGet("freelancer-details")]
        public async Task<IActionResult> GetFreelancerDetails()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _profileSettingsService.GetFreelancerDetailsAsync(userId);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves the public profile details of a user by their hashed user ID.
        /// </summary>
        /// <param name="userIdHash">The hashed user ID.</param>
        /// <returns>The public profile details.</returns>
        [HttpGet("public/{userIdHash}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(string userIdHash)
        {
            var response = await _profileSettingsService.GetPublicProfileAsync(userIdHash);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(response);
        }

        /// <summary>
        /// Updates the full name of the logged-in user.
        /// </summary>
        /// <param name="fullname">The new full name.</param>
        /// <returns>A status indicating success and the updated name.</returns>
        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] string fullname)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateFullNameAsync(userId, fullname);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Name updated successfully.",  data = response.Data });
        }

        /// <summary>
        /// Initiates or completes the email update process for the logged-in user.
        /// </summary>
        /// <param name="email">The new email address.</param>
        /// <returns>A status indicating success.</returns>
        [HttpPatch("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);

            var response = await _profileSettingsService.UpdateEmailAsync(userId, email);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = response.Message, data = response.Data });
        }

        /// <summary>
        /// Updates the professional title of the logged-in freelancer.
        /// </summary>
        /// <param name="title">The new professional title.</param>
        /// <returns>A status indicating success and the updated title.</returns>
        [HttpPatch("title")]
        public async Task<IActionResult> UpdateTitle([FromBody] string title)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateTitleAsync(userId, title);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Title updated successfully.", data = response.Data });
        }

        /// <summary>
        /// Updates the professional biography of the logged-in freelancer.
        /// </summary>
        /// <param name="bio">The new biography text.</param>
        /// <returns>A status indicating success and the updated biography.</returns>
        [HttpPatch("bio")]
        public async Task<IActionResult> UpdateBio([FromBody] string? bio)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateBioAsync(userId, bio);
            if (!response.Succeeded) return NotFound(response.Errors);
            return Ok(new { message = "Bio updated successfully.", data = response.Data });
        }

        /// <summary>
        /// Updates the experience level of the logged-in freelancer.
        /// </summary>
        /// <param name="dto">The updated experience level details.</param>
        /// <returns>A status indicating success and the updated experience level.</returns>
        [HttpPatch("experience-level")]
        public async Task<IActionResult> UpdateExperienceLevel([FromBody] ExperienceUpdateDto dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateExperienceAsync(userId, dto);
            if (!response.Succeeded) return BadRequest(response.Errors);
            return Ok(new { message = "Experience updated successfully.", data = response.Data });
        }

        /// <summary>
        /// Creates a new billing payment method for the logged-in user.
        /// </summary>
        /// <param name="dto">The payment method details.</param>
        /// <returns>A status indicating success and the created payment method details.</returns>
        [HttpPost("payment-method")]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethodCreateDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.CreateBillingAsync(userId, dto);
            if (!response.Succeeded) return NotFound("User not found.");
            return Ok(new { message = "Billing information created successfully.", data = response.Data });
        }

        /// <summary>
        /// Updates an existing billing payment method by its ID.
        /// </summary>
        /// <param name="id">The payment method ID.</param>
        /// <param name="dto">The updated payment method details.</param>
        /// <returns>The updated payment method details.</returns>
        [HttpPut("payment-method/{id}")]
        public async Task<IActionResult> UpdatePaymentMethod([FromRoute] string id, [FromBody] PaymentMethodUpdateDTO dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateBillingAsync(userId, id, dto);
            if (!response.Succeeded) return NotFound("User not found.");
            return Ok(response.Data);
        }

        /// <summary>
        /// Deletes a billing payment method by its ID.
        /// </summary>
        /// <param name="id">The payment method ID.</param>
        /// <returns>No content on success.</returns>
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

        /// <summary>
        /// Updates the location information for the logged-in user.
        /// </summary>
        /// <param name="dto">The updated location details.</param>
        /// <returns>A status indicating success.</returns>
        [HttpPatch("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var response = await _profileSettingsService.UpdateLocationAsync(userId, dto);

            if (!response.Succeeded) return NotFound("User not found.");

            return Ok(new { message = "Location updated successfully." });
        }

        /// <summary>
        /// Updates general freelancer-specific details for the logged-in user.
        /// </summary>
        /// <param name="dto">The updated freelancer details.</param>
        /// <returns>A status indicating success and the updated freelancer details.</returns>
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
