using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetMyPendingSpecialistReviewsQueryHandler : IRequestHandler<GetMyPendingSpecialistReviewsQuery, Result<List<ContractSpecialistReviewReadDto>>>
    {
        private readonly AppDbContext _context;

        public GetMyPendingSpecialistReviewsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ContractSpecialistReviewReadDto>>> Handle(GetMyPendingSpecialistReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _context.ContractSpecialistReviews
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Contract)
                        .ThenInclude(c => c.Proposal)
                            .ThenInclude(p => p.JobPost)
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Contract)
                        .ThenInclude(c => c.JobPost)
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Attachments)
                .Where(r => r.AssignedSpecialistId == request.SpecialistId &&
                            r.Status == SpecialistReviewStatus.InProgress)
                .OrderBy(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            var data = reviews.Select(r => r.ToReadDto()).ToList();

            return new Result<List<ContractSpecialistReviewReadDto>>
            {
                Succeeded = true,
                Data = data
            };
        }
    }
}
