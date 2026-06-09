using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RevisionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RevisionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("open")]
        [Authorize(Roles = "Specialist")]
        [ProducesResponseType(typeof(IEnumerable<RevisionRequestDto>), 200)]
        public async Task<IActionResult> GetOpenRevisions()
        {
            var result = await _mediator.Send(new GetRevisionRequestsQuery());
            return Ok(result);
        }
    }
}
