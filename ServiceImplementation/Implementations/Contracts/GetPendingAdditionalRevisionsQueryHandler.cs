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
    public class GetPendingAdditionalRevisionsQueryHandler : IRequestHandler<GetPendingAdditionalRevisionsQuery, Result<IEnumerable<AdditionalRevisionRequestDto>>>
    {
        private readonly AppDbContext _context;

        public GetPendingAdditionalRevisionsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<AdditionalRevisionRequestDto>>> Handle(GetPendingAdditionalRevisionsQuery request, CancellationToken cancellationToken)
        {
            var requests = await _context.AdditionalRevisionRequests
                .Include(r => r.Contract)
                .Include(r => r.Client)
                .Where(r => r.Contract.FreelancerId == request.FreelancerId && r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            var dtos = requests.Select(r => new AdditionalRevisionRequestDto
            {
                Id = r.Id,
                ContractId = r.ContractId,
                DeliveryId = r.DeliveryId,
                RequestedCount = r.RequestedCount,
                ClientId = r.ClientId,
                ClientName = r.Client.FullName,
                Reason = r.Reason,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                RespondedAt = r.RespondedAt
            });

            return new Result<IEnumerable<AdditionalRevisionRequestDto>>
            {
                Succeeded = true,
                Data = dtos
            };
        }
    }
}