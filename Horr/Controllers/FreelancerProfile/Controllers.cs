using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.FreelancerProfile;
using ServiceContracts.FreelancerProfile;
using System.Linq;

namespace Horr.Controllers.FreelancerProfile
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
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
        public async Task<IActionResult> GetPortfolio()
        {
            var userId = GetCurrentUserId();
            var result = await _portfolioService.GetUserPortfolioAsync(userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolioItem([FromBody] PortfolioCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _portfolioService.CreatePortfolioItemAsync(userId, dto);
            return CreatedAtAction(nameof(GetPortfolio), new { id = result.Id }, result);
        }
    }

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
