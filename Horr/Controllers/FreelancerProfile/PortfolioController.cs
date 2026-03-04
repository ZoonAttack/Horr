using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.FreelancerProfile;
using Services.Freelancer.FreelancerProfile;

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
}
