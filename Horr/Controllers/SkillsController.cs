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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
        {
            var result = await _skillService.GetAllSkillsAsync();
            return Ok(result.Data);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByCategory(string categoryId)
        {
            var result = await _skillService.GetSkillsByCategoryAsync(categoryId);
            return Ok(result.Data);
        }

        [HttpGet("my-skills")]
        public async Task<ActionResult<IEnumerable<FreelancerSkillDto>>> GetMySkills()
        {
            var userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _skillService.GetMySkillsAsync(userId);

            if (!result.Succeeded)
                return result.ErrorCode == "FREELANCER_NOT_FOUND" ? NotFound(result) : BadRequest(result);

            return Ok(result.Data);
        }

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
