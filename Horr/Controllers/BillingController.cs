using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Implementations.Wallet;
using System.Security.Claims;
using Services;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BillingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a deposit request (manual bank transfer proof upload).
        /// </summary>
        /// <param name="command">The details and proof file of the deposit.</param>
        /// <returns>The created deposit request details.</returns>
        [HttpPost("deposit-requests")]
        [Authorize(Policy = "ClientOnly")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DepositRequestDto>> SubmitDeposit([FromForm] SubmitDepositRequestCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // The command already has properties from form, but we ensure ClientId is set from auth
            var result = await _mediator.Send(command with { ClientId = userId });
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetMyDeposits), null, result.Data);
        }

        /// <summary>
        /// Retrieves deposit requests submitted by the logged-in client.
        /// </summary>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of deposit requests.</returns>
        [HttpGet("deposit-requests/my-requests")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<ActionResult<PagedResult<DepositRequestDto>>> GetMyDeposits([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyDepositRequestsQuery(userId, page, pageSize));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Submits a withdrawal request.
        /// </summary>
        /// <param name="command">The details of the withdrawal request.</param>
        /// <returns>The created withdrawal request details.</returns>
        [HttpPost("withdrawal-requests")]
        [Authorize(Policy = "FreelancerOnly")]
        public async Task<ActionResult<WithdrawalRequestDto>> SubmitWithdrawal([FromBody] SubmitWithdrawalRequestCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(command with { FreelancerId = userId });
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetMyWithdrawals), null, result.Data);
        }

        /// <summary>
        /// Retrieves withdrawal requests submitted by the logged-in freelancer.
        /// </summary>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <returns>A paged list of withdrawal requests.</returns>
        [HttpGet("withdrawal-requests/my-requests")]
        [Authorize(Policy = "FreelancerOnly")]
        public async Task<ActionResult<PagedResult<WithdrawalRequestDto>>> GetMyWithdrawals([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyWithdrawalRequestsQuery(userId, page, pageSize));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves the current wallet balance of the logged-in user.
        /// </summary>
        /// <returns>The wallet balance details.</returns>
        [HttpGet("wallet-balance")]
        [Authorize]
        public async Task<ActionResult<WalletBalanceDto>> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetWalletBalanceQuery(userId));
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result.Data);
        }
    }
}
