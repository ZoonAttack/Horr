using Entities;
using Entities.Skill;
using Horr.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTOs.Skill;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        /// <summary>
        /// Retrieves all skills available in the system.
        /// </summary>
        /// <returns>A list of all skills.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
        {
            var result = await _skillService.GetAllSkillsAsync();
            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves skills under a specific category.
        /// </summary>
        /// <param name="categoryId">The category ID to filter skills.</param>
        /// <returns>A list of skills in the specified category.</returns>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByCategory(string categoryId)
        {
            var result = await _skillService.GetSkillsByCategoryAsync(categoryId);
            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves the list of skills associated with the logged-in freelancer.
        /// </summary>
        /// <returns>A list of the freelancer's skills.</returns>
        [HttpGet("my-skills")]
        public async Task<ActionResult<IEnumerable<FreelancerSkillDto>>> GetMySkills()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _skillService.GetMySkillsAsync(userId);

            if (!result.Succeeded)
                return result.ErrorCode == "FREELANCER_NOT_FOUND" ? NotFound(result) : BadRequest(result);

            return Ok(result.Data);
        }

        /// <summary>
        /// Adds a skill to the logged-in freelancer's profile.
        /// </summary>
        /// <param name="dto">The details of the skill to add.</param>
        /// <returns>The added freelancer skill details.</returns>
        [HttpPost("my-skills")]
        public async Task<ActionResult<FreelancerSkillDto>> AddMySkill([FromBody] AddFreelancerSkillDto dto)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _skillService.AddMySkillAsync(userId, dto);

            if (!result.Succeeded)
            {
                if (result.ErrorCode == "FREELANCER_NOT_FOUND" || result.ErrorCode == "SKILL_NOT_FOUND")
                    return NotFound(result);
                if (result.ErrorCode == "SKILL_ALREADY_ADDED")
                    return Conflict(result);
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetMySkills), result.Data);
        }

        /// <summary>
        /// Removes a skill from the logged-in freelancer's profile.
        /// </summary>
        /// <param name="skillId">The skill ID to remove.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("my-skills/{skillId}")]
        public async Task<IActionResult> DeleteMySkill(string skillId)
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _skillService.DeleteMySkillAsync(userId, skillId);

            if (!result.Succeeded)
                return NotFound(result);

            return NoContent();
        }
    }
}
