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

        private Guid GetCurrentUserId()
        {
            var userIdVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdVal, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid User ID");
        }

        [HttpGet]
        public async Task<IActionResult> GetExperience()
        {
            var userId = GetCurrentUserId();
            var result = await _experienceService.GetUserExperienceAsync(userId);
            
            if (result == null || !result.Any())
            {
                return Ok(Array.Empty<ExperienceResponseDto>());
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExperience(Guid id)
        {
            var success = await _experienceService.SoftDeleteExperienceAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
