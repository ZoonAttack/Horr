using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.FreelancerProfile;
using System.Linq;
using Services.Freelancer.FreelancerProfile;

namespace Horr.Controllers.FreelancerProfile
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperienceController : ControllerBase
    {
        private readonly IExperienceService _experienceService;

        public ExperienceController(IExperienceService experienceService)
        {
            _experienceService = experienceService;
        }

        private string GetCurrentUserId()
        {
            var userIdVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdVal))
            {
                return userIdVal;
            }
            throw new UnauthorizedAccessException("Invalid User ID");
        }

        [HttpGet]
        public async Task<IActionResult> GetExperience()
        {
            var userId = GetCurrentUserId();
            var result = await _experienceService.GetUserExperienceAsync(userId);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExperience(string id)
        {
            var result = await _experienceService.SoftDeleteExperienceAsync(id);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return NoContent();
        }
    }
}
