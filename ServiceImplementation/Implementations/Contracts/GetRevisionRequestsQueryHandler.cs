using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetRevisionRequestsQueryHandler : IRequestHandler<GetRevisionRequestsQuery, List<RevisionRequestDto>>
    {
        private readonly AppDbContext _context;

        public GetRevisionRequestsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RevisionRequestDto>> Handle(GetRevisionRequestsQuery request, CancellationToken cancellationToken)
        {
            var revisions = await _context.RevisionRequests
                .Where(r => r.Status == RevisionStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            return revisions.Select(r => r.ToDto()).ToList();
        }
    }
}
