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

        /// <summary>
        /// Retrieves a list of all pending deposit requests. Only accessible by Admins.
        /// </summary>
        /// <returns>A list of pending deposit requests.</returns>
        [HttpGet("deposit-requests/pending")]
        public async Task<ActionResult<IEnumerable<DepositRequestDto>>> GetPendingDeposits()
        {
            var result = await _mediator.Send(new GetPendingDepositRequestsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Approves or rejects a pending deposit request. Only accessible by Admins.
        /// </summary>
        /// <param name="id">The deposit request ID.</param>
        /// <param name="dto">The review decision status and administrative note.</param>
        /// <returns>The reviewed deposit request details.</returns>
        [HttpPatch("deposit-requests/{id}/review")]
        public async Task<ActionResult<DepositRequestDto>> ReviewDeposit(string id, [FromBody] ReviewDepositRequestCommandDto dto)
        {
            var result = await _mediator.Send(new ReviewDepositRequestCommand(id, dto.Status, dto.AdminNote));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a list of all pending withdrawal requests. Only accessible by Admins.
        /// </summary>
        /// <returns>A list of pending withdrawal requests.</returns>
        [HttpGet("withdrawal-requests/pending")]
        public async Task<ActionResult<IEnumerable<WithdrawalRequestDto>>> GetPendingWithdrawals()
        {
            var result = await _mediator.Send(new GetPendingWithdrawalRequestsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Approves or rejects a pending withdrawal request. Only accessible by Admins.
        /// </summary>
        /// <param name="id">The withdrawal request ID.</param>
        /// <param name="dto">The review decision status and administrative note.</param>
        /// <returns>The reviewed withdrawal request details.</returns>
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
