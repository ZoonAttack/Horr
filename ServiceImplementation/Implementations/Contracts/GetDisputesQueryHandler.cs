using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, Result<PagedResult<DisputeAdminDto>>>
    {
        private readonly AppDbContext _context;

        public GetDisputesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<DisputeAdminDto>>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Disputes
                .Include(d => d.Contract).ThenInclude(c => c.Client)
                .Include(d => d.Contract).ThenInclude(c => c.Freelancer)
                .Include(d => d.ContractDelivery).ThenInclude(d => d.Attachments)
                .Include(d => d.OpenedByUser)
                .OrderByDescending(d => d.OpenedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new DisputeAdminDto
                {
                    Id = d.Id,
                    Status = d.Status,
                    Reason = d.Reason,
                    OpenedAt = d.OpenedAt,
                    OpenedByUserId = d.OpenedByUserId,
                    OpenedByUserFullName = d.OpenedByUser.FullName,
                    AdminDecision = d.AdminDecision,
                    ResolvedAt = d.ResolvedAt,
                    ContractId = d.ContractId,
                    ClientId = d.Contract.ClientId,
                    ClientFullName = d.Contract.Client.FullName,
                    FreelancerId = d.Contract.FreelancerId,
                    FreelancerFullName = d.Contract.Freelancer.FullName,
                    AgreedRate = d.Contract.AgreedRate,
                    ContractStatus = d.Contract.Status,
                    DeliveryId = d.ContractDeliveryId,
                    Attachments = d.ContractDelivery.Attachments
                        .Where(a => !a.IsDeleted)
                        .Select(a => new AttachmentSummaryDto
                        {
                            Id = a.Id,
                            OriginalFileName = a.OriginalFileName,
                            FileType = a.FileType,
                            FileSizeBytes = a.FileSizeBytes
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<DisputeAdminDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return new Result<PagedResult<DisputeAdminDto>>
            {
                Succeeded = true,
                Data = pagedResult
            };
        }
    }
}
