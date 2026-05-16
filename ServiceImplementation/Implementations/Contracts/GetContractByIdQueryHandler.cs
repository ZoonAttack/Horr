using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, Result<ContractReadDTO>>
    {
        private readonly AppDbContext _context;

        public GetContractByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ContractReadDTO>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
        {
            var contract = await _context.Contracts
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.JobPost)
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.WorkDeliveries)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<ContractReadDTO> { Succeeded = false, ErrorCode = ErrorCodes.ContractNotFound, Message = "Contract not found." };
            }

            // Check if user is part of the contract
            if (contract.ClientId != request.UserId && contract.FreelancerId != request.UserId)
            {
                return new Result<ContractReadDTO> { Succeeded = false, ErrorCode = ErrorCodes.Unauthorized, Message = "Unauthorized." };
            }

            var dto = new ContractReadDTO
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                Proposal_Title = contract.Proposal?.JobPost?.Title,
                Client_Name = contract.Client?.FullName,
                Freelancer_Name = contract.Freelancer?.FullName,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                ClosedAt = contract.ClosedAt,
                CreatedAt = contract.CreatedAt,
                LatestDeliverySummary = contract.WorkDeliveries
                    .OrderByDescending(d => d.SubmittedAt)
                    .Select(d => d.Note)
                    .FirstOrDefault()
            };

            return new Result<ContractReadDTO> { Succeeded = true, Data = dto };
        }
    }
}
