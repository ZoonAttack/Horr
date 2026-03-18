using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeliverWorkCommandHandler : IRequestHandler<DeliverWorkCommand, WorkDeliveryDto>
    {
        private readonly AppDbContext _context;

        public DeliverWorkCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WorkDeliveryDto> Handle(DeliverWorkCommand request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            if (contract.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: Only the contract freelancer can deliver work.");
            }

            // State Guard
            ContractStateGuard.EnsureCanDeliverWork(contract);

            // Create delivery
            var delivery = new WorkDelivery
            {
                ContractId = contract.Id,
                Note = request.Note,
                SubmittedAt = DateTime.UtcNow,
                ActionStatus = ActionStatus.NeedsAttention
            };

            _context.WorkDeliveries.Add(delivery);
            await _context.SaveChangesAsync(cancellationToken);

            return new WorkDeliveryDto
            {
                Id = delivery.Id,
                ContractId = delivery.ContractId,
                Note = delivery.Note,
                ActionStatus = delivery.ActionStatus,
                SubmittedAt = delivery.SubmittedAt
            };
        }
    }
}
