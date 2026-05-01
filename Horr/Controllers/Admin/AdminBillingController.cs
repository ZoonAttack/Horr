using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Implementations.Wallet;
using Entities.Enums;

namespace Horr.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/billing")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminBillingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminBillingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("deposit-requests/pending")]
        public async Task<ActionResult<IEnumerable<DepositRequestDto>>> GetPendingDeposits()
        {
            var result = await _mediator.Send(new GetPendingDepositRequestsQuery());
            return Ok(result);
        }

        [HttpPatch("deposit-requests/{id}/review")]
        public async Task<ActionResult<DepositRequestDto>> ReviewDeposit(string id, [FromBody] ReviewDepositRequestCommandDto dto)
        {
            var result = await _mediator.Send(new ReviewDepositRequestCommand(id, dto.Status, dto.AdminNote));
            return Ok(result);
        }

        [HttpGet("withdrawal-requests/pending")]
        public async Task<ActionResult<IEnumerable<WithdrawalRequestDto>>> GetPendingWithdrawals()
        {
            var result = await _mediator.Send(new GetPendingWithdrawalRequestsQuery());
            return Ok(result);
        }

        [HttpPatch("withdrawal-requests/{id}/review")]
        public async Task<ActionResult<WithdrawalRequestDto>> ReviewWithdrawal(string id, [FromBody] ReviewWithdrawalRequestCommandDto dto)
        {
            var result = await _mediator.Send(new ReviewWithdrawalRequestCommand(id, dto.Status, dto.AdminNote));
            return Ok(result);
        }
    }

    public record ReviewDepositRequestCommandDto(DepositStatus Status, string? AdminNote);
    public record ReviewWithdrawalRequestCommandDto(WithdrawalStatus Status, string? AdminNote);
}
