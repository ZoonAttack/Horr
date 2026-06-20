using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
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
                .Include(c => c.ContractDeliveries)
                .Include(c => c.ContractMilestones)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                contract = await _context.Contracts
                    .Include(c => c.Proposal)
                        .ThenInclude(p => p.JobPost)
                    .Include(c => c.Client)
                    .Include(c => c.Freelancer)
                    .Include(c => c.WorkDeliveries)
                    .Include(c => c.ContractDeliveries)
                    .Include(c => c.ContractMilestones)
                    .FirstOrDefaultAsync(c => c.ProposalId == request.ContractId, cancellationToken);
            }

            if (contract == null)
            {
                return new Result<ContractReadDTO> { Succeeded = false, ErrorCode = ErrorCodes.ContractNotFound, Message = "Contract not found." };
            }

            // Check if user is part of the contract
            if (contract.ClientId != request.UserId && contract.FreelancerId != request.UserId)
            {
                return new Result<ContractReadDTO> { Succeeded = false, ErrorCode = ErrorCodes.Unauthorized, Message = "Unauthorized." };
            }

            var hasActiveDispute = await _context.Disputes
                .AnyAsync(d => d.ContractId == contract.Id && (d.Status == DisputeStatus.Open || d.Status == DisputeStatus.UnderReview), cancellationToken);

            var dto = new ContractReadDTO
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                Proposal_Title = contract.Proposal?.JobPost?.Title ?? contract.JobPost?.Title ?? "Direct Offer",
                Client_Name = contract.Client?.FullName,
                Freelancer_Name = contract.Freelancer?.FullName,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                ClosedAt = contract.ClosedAt,
                CreatedAt = contract.CreatedAt,
                DueDate = contract.DueDate,
                MaxRevisions = contract.MaxRevisions,
                Description = contract.CustomJobDescription ?? contract.Proposal?.JobPost?.Description ?? string.Empty,
                Milestones = contract.ContractMilestones?
                    .Where(m => !m.IsDeleted)
                    .Select(m => new ContractMilestoneDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        Amount = m.Amount,
                        DueDate = m.DueDate,
                        Status = m.Status.ToString()
                    }).ToList() ?? new List<ContractMilestoneDto>(),
                LatestDeliverySummary = contract.ContractDeliveries
                    .OrderByDescending(d => d.SubmittedAt)
                    .Select(d => d.DeliveryNote)
                    .FirstOrDefault() ?? contract.WorkDeliveries
                    .OrderByDescending(d => d.SubmittedAt)
                    .Select(d => d.Note)
                    .FirstOrDefault(),
                InDispute = hasActiveDispute
            };

            return new Result<ContractReadDTO> { Succeeded = true, Data = dto };
        }
    }
}
