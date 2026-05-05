using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using Services;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetMyContractsQueryHandler : IRequestHandler<GetMyContractsQuery, Services.PagedResult<ContractReadDTO>>
    {
        private readonly AppDbContext _context;

        public GetMyContractsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Services.PagedResult<ContractReadDTO>> Handle(GetMyContractsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Contracts.AsQueryable();

            if (request.UserRole == "Client")
            {
                query = query.Where(c => c.ClientId == request.UserId);
            }
            else
            {
                query = query.Where(c => c.FreelancerId == request.UserId);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ContractReadDTO
                {
                    Id = c.Id,
                    ProposalId = c.ProposalId,
                    ClientId = c.ClientId,
                    FreelancerId = c.FreelancerId,
                    Proposal_Title = c.Proposal != null ? c.Proposal.JobPost.Title : (c.JobPost != null ? c.JobPost.Title : "Direct Offer"),
                    Client_Name = c.Client.FullName,
                    Freelancer_Name = c.Freelancer.FullName,
                    AgreedRate = c.AgreedRate,
                    Status = c.Status,
                    StartedAt = c.StartedAt,
                    ClosedAt = c.ClosedAt,
                    CreatedAt = c.CreatedAt,
                    // Latest delivery summary logic: most recent delivery note
                    LatestDeliverySummary = c.WorkDeliveries
                        .OrderByDescending(d => d.SubmittedAt)
                        .Select(d => d.Note)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return new Services.PagedResult<ContractReadDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
            };
        }
    }
}
