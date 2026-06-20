using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Services.Wallet;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class ApproveDeliveryCommandHandler : IRequestHandler<ApproveDeliveryCommand, Result<ContractDeliveryDto>>
    {
        private readonly AppDbContext _context;
        private readonly IEscrowService _escrowService;

        public ApproveDeliveryCommandHandler(AppDbContext context, IEscrowService escrowService)
        {
            _context = context;
            _escrowService = escrowService;
        }

        public async Task<Result<ContractDeliveryDto>> Handle(ApproveDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _context.ContractDeliveries
                .Include(d => d.Attachments)
                .Include(d => d.RevisionRequests)
                .Include(d => d.AdditionalRevisionRequests)
                    .ThenInclude(arr => arr.Client)
                .Include(d => d.SpecialistReviews)
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if (delivery == null)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = $"Delivery with ID {request.DeliveryId} not found.",
                    Errors = new List<string> { $"Delivery with ID {request.DeliveryId} not found." }
                };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = "Associated contract not found.",
                    Errors = new List<string> { "Associated contract not found." }
                };
            }

            // Who: Client only
            if (contract.ClientId != request.ClientId)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = "Only the contract client can approve the delivery.",
                    Errors = new List<string> { "Only the contract client can approve the delivery." }
                };
            }

            if (delivery.Status != DeliveryStatus.Pending)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = "Only pending deliveries can be approved.",
                    Errors = new List<string> { "Only pending deliveries can be approved." }
                };
            }

            // Check if there is an active escrow transaction before releasing it
            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == delivery.ContractMilestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded, cancellationToken);

            if (escrowTx == null)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = "No active escrow transaction found to release.",
                    Errors = new List<string> { "No active escrow transaction found to release." }
                };
            }

            // Update status to Approved
            delivery.Status = DeliveryStatus.Approved;
            delivery.CompletedAt = DateTime.UtcNow;

            // Release escrow funds to freelancer
            var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");

            var releaseResult = await _escrowService.ReleaseToFreelancerAsync(contractGuid, delivery.ContractMilestoneId);
            if (!releaseResult.Succeeded)
            {
                return new Result<ContractDeliveryDto>
                {
                    Succeeded = false,
                    Message = releaseResult.Message,
                    Errors = releaseResult.Errors
                };
            }

            // Auto-complete and close the contract
            contract.Status = ContractStatus.Completed;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result<ContractDeliveryDto>
            {
                Succeeded = true,
                Data = delivery.ToDto()
            };
        }
    }
}
