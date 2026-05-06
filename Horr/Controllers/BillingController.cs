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
