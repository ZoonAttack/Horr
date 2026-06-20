using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetContractDeliveriesQueryHandler : IRequestHandler<GetContractDeliveriesQuery, List<ContractDeliveryDto>>
    {
        private readonly AppDbContext _context;

        public GetContractDeliveriesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContractDeliveryDto>> Handle(GetContractDeliveriesQuery request, CancellationToken cancellationToken)
        {
            var deliveries = await _context.ContractDeliveries
                .Include(d => d.Attachments)
                .Include(d => d.RevisionRequests)
                .Include(d => d.AdditionalRevisionRequests)
                    .ThenInclude(arr => arr.Client)
                .Include(d => d.SpecialistReviews)
                .Where(d => d.ContractId == request.ContractId)
                .OrderByDescending(d => d.SubmittedAt)
                .ToListAsync(cancellationToken);

            return deliveries.Select(d => d.ToDto()).ToList();
        }
    }
}
