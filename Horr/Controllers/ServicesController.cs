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

        [HttpGet("my-services")]
        [ProducesResponseType(typeof(ServiceGroupedDto), 200)]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyServicesQuery(userId));
            return Ok(result);
        }

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
