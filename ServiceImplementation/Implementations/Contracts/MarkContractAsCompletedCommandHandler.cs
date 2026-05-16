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
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class MarkContractAsCompletedCommandHandler : IRequestHandler<MarkContractAsCompletedCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public MarkContractAsCompletedCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(MarkContractAsCompletedCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Client account not found or is deleted."
                };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = $"Contract with ID {request.ContractId} not found."
                };
            }

            if (contract.ClientId != request.ClientId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only the client can complete the contract."
                };
            }

            // State Guard
            try 
            {
                ContractStateGuard.EnsureCanComplete(contract);
            }
            catch (Exception ex)
            {
                return new Result<bool> { Succeeded = false, ErrorCode = ErrorCodes.InvalidState, Message = ex.Message };
            }

            contract.Status = ContractStatus.Completed;
            contract.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
