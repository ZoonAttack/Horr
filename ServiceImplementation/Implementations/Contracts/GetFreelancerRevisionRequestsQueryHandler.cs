using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetFreelancerRevisionRequestsQueryHandler : IRequestHandler<GetFreelancerRevisionRequestsQuery, Result<List<RevisionRequestDto>>>
    {
        private readonly AppDbContext _context;

        public GetFreelancerRevisionRequestsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<RevisionRequestDto>>> Handle(GetFreelancerRevisionRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RevisionRequests
                .Include(r => r.Delivery)
                    .ThenInclude(d => d.Contract)
                .Where(r => r.Delivery.Contract.FreelancerId == request.FreelancerId);

            if (request.ContractId.HasValue)
            {
                query = query.Where(r => r.Delivery.ContractId == request.ContractId.Value);
            }

            var revisions = await query
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            var dtos = revisions.Select(r => r.ToDto()).ToList();

            return new Result<List<RevisionRequestDto>>
            {
                Succeeded = true,
                Data = dtos
            };
        }
    }
}
