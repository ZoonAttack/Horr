using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RejectContractCommandHandler : IRequestHandler<RejectContractCommand, bool>
    {
        private readonly AppDbContext _context;

        public RejectContractCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RejectContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new NotFoundException($"Contract with ID {request.ContractId} not found.");
            }

            if (contract.ClientId != request.ClientId)
            {
                throw new UnauthorizedAccessException("Unauthorized: Only the client can reject the contract.");
            }

            // State Guard
            ContractStateGuard.EnsureCanRejectContract(contract);

            contract.Status = ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
