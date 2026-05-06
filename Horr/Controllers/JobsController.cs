using Entities.Users;
using Horr.Extentions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Implementations.JobManagement;
using Services.Client;
using System.Security.Claims;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJobService _jobService;

        public JobsController(IMediator mediator, IJobService jobService)
        {
            _mediator = mediator;
            _jobService = jobService;
        }

        [HttpGet("jobs")]
        public async Task<ActionResult<SearchJobsQueryResponse>> GetJobs([FromQuery] SearchJobsQuery query)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _mediator.Send(query with { CurrentUserId = userId });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpPost("create-job")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> CreateJob([FromBody] JobDetailsDto jobDetails)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _jobService.CreateJobAsync(userId, jobDetails);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetJob), new { id = result.Data.Id }, result.Data);
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("jobs/{id}")]
        public async Task<ActionResult<JobDetailsDto>> GetJob(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _mediator.Send(new GetJobDetailsQuery(id, userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpPost("{id}/save-job")]
        [Authorize(Policy ="FreelancerOnly")]
        public async Task<IActionResult> SaveJob(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new ToggleSavedJobCommand(id, userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpDelete("{id}/unsave-job")]
        [Authorize(Policy = "FreelancerOnly")]
        public async Task<IActionResult> UnsaveJob(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new ToggleSavedJobCommand(id, userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        [HttpGet("{id}/proposals")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> GetJobProposals(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new ServiceImplementation.Implementations.Proposals.GetProposalsForJobQuery(id, userId, page, pageSize));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }
    }
}
