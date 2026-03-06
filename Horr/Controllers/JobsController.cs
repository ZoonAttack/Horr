using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Implementations.JobManagement;
using System.Security.Claims;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SearchJobsQueryResponse>> GetJobs([FromQuery] SearchJobsQuery query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(query with { CurrentUserId = userId });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobDetailsDto>> GetJob(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new GetJobDetailsQuery(id, userId));
            return Ok(result);
        }

        [HttpPost("{id}/save")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> SaveJob(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new ToggleSavedJobCommand(id, userId));
            return NoContent();
        }

        [HttpDelete("{id}/save")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> UnsaveJob(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new ToggleSavedJobCommand(id, userId));
            return NoContent();
        }
    }
}
