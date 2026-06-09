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

        /// <summary>
        /// Searches and filters active job listings.
        /// </summary>
        /// <param name="query">The search and filter parameters.</param>
        /// <returns>A paged list of search results.</returns>
        [HttpGet("jobs")]
        public async Task<ActionResult<SearchJobsQueryResponse>> GetJobs([FromQuery] SearchJobsQuery query)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _mediator.Send(query with { CurrentUserId = userId });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new job post. Only accessible by Clients.
        /// </summary>
        /// <param name="jobDetails">The job post details.</param>
        /// <returns>The created job post details.</returns>
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

        /// <summary>
        /// Retrieves the details of a specific job post by ID.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <returns>The job details.</returns>
        [HttpGet("jobs/{id}")]
        public async Task<ActionResult<JobDetailsDto>> GetJob(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            var result = await _mediator.Send(new GetJobDetailsQuery(id, userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Saves a job post to the freelancer's saved list.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <returns>The updated saved status or details.</returns>
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

        /// <summary>
        /// Removes a job post from the freelancer's saved list.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <returns>The updated saved status or details.</returns>
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

        /// <summary>
        /// Retrieves proposals submitted for a specific job post. Only accessible by Clients.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of proposals.</returns>
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

        /// <summary>
        /// Updates an existing job post. Only accessible by Clients.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <param name="jobDetails">The updated job details.</param>
        /// <returns>The updated job details.</returns>
        [HttpPut("update-job/{id}")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> UpdateJob(string id, [FromBody] JobDetailsDto jobDetails)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _jobService.UpdateJobAsync(userId, id, jobDetails);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Deletes a job post by its ID. Only accessible by Clients.
        /// </summary>
        /// <param name="id">The job post ID.</param>
        /// <returns>The deletion status result.</returns>
        [HttpDelete("delete-job/{id}")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> DeleteJob(string id)
        {
            string userId = ClaimsPrincipalExtensions.GetLoggedInUserId<string>(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _jobService.DeleteJobAsync(userId, id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
