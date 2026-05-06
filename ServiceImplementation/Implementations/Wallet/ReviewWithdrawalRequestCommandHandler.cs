using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ServiceImplementation.Implementations.Wallet
{
    public class ReviewWithdrawalRequestCommandHandler : IRequestHandler<ReviewWithdrawalRequestCommand, Result<WithdrawalRequestDto>>
    {
        private readonly AppDbContext _context;

        public ReviewWithdrawalRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<WithdrawalRequestDto>> Handle(ReviewWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var withdrawalRequest = await _context.WithdrawalRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (withdrawalRequest == null)
            {
                throw new NotFoundException("Withdrawal request not found.");
            }

            if (withdrawalRequest.Status != WithdrawalStatus.Pending)
            {
                throw new InvalidStateException("Only pending withdrawal requests can be reviewed.");
            }

            withdrawalRequest.Status = request.Status;
            withdrawalRequest.ReviewedAt = DateTime.UtcNow;
            withdrawalRequest.AdminNote = request.AdminNote;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result<WithdrawalRequestDto> { Succeeded = true, Data = withdrawalRequest.ToDto() };
        }
    }
}
