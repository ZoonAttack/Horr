using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Implementations.Marketplace;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new service catalog listing.
        /// </summary>
        /// <param name="dto">The service details.</param>
        /// <param name="images">Optional list of service images.</param>
        /// <param name="video">Optional service video file.</param>
        /// <param name="documents">Optional supporting documents.</param>
        /// <param name="coverImageFileName">Optional file name of the cover image.</param>
        /// <returns>The created service catalog item details.</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ServiceCatalogItemDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create(
            [FromForm] ServiceCreateDTO dto,
            [FromForm] List<IFormFile>? images,
            [FromForm] IFormFile? video,
            [FromForm] List<IFormFile>? documents,
            [FromForm] string? coverImageFileName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Ensure FreelancerId in DTO matches the user
            dto.FreelancerId = userId;

            var result = await _mediator.Send(new CreateServiceCommand(dto, images, video, documents, coverImageFileName));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing service catalog listing.
        /// </summary>
        /// <param name="id">The service catalog item ID.</param>
        /// <param name="dto">The updated service details.</param>
        /// <param name="images">Optional updated service images.</param>
        /// <param name="video">Optional updated service video file.</param>
        /// <param name="documents">Optional updated supporting documents.</param>
        /// <param name="coverImageFileName">Optional updated cover image file name.</param>
        /// <returns>The updated service catalog item details.</returns>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ServiceCatalogItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(
            string id,
            [FromForm] ServiceUpdateDTO dto,
            [FromForm] List<IFormFile>? images,
            [FromForm] IFormFile? video,
            [FromForm] List<IFormFile>? documents,
            [FromForm] string? coverImageFileName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            dto.Id = id;
            dto.FreelancerId = userId;

            var result = await _mediator.Send(new UpdateServiceCommand(dto, images, video, documents, coverImageFileName));
            return Ok(result);
        }

        /// <summary>
        /// Deletes a service catalog listing by its ID.
        /// </summary>
        /// <param name="id">The service catalog item ID.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _mediator.Send(new DeleteServiceCommand(id, userId));
            return NoContent();
        }

        /// <summary>
        /// Retrieves service listings belonging to the logged-in freelancer.
        /// </summary>
        /// <returns>Grouped service catalog listings details.</returns>
        [HttpGet("my-services")]
        [ProducesResponseType(typeof(ServiceGroupedDto), 200)]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyServicesQuery(userId));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a service catalog listing by its ID.
        /// </summary>
        /// <param name="id">The service catalog item ID.</param>
        /// <returns>The service details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ServiceCatalogItemDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetServiceByIdQuery(id, userId));
            return Ok(result);
        }
    }
}
