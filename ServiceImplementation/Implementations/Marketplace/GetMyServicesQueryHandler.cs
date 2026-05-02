using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceContracts.DTOs.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class GetMyServicesQueryHandler : IRequestHandler<GetMyServicesQuery, ServiceGroupedDto>
    {
        private readonly AppDbContext _context;

        public GetMyServicesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceGroupedDto> Handle(GetMyServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.GalleryFiles)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .Where(s => s.FreelancerId == request.FreelancerId && !s.IsDeleted)
                .ToListAsync(cancellationToken);

            var result = new ServiceGroupedDto();

            result.Approved = services
                .Where(s => s.Status == ServiceStatus.Approved)
                .Select(s => s.ToDto())
                .ToList();

            result.UnderReview = services
                .Where(s => s.Status == ServiceStatus.UnderReview)
                .Select(s => s.ToDto())
                .ToList();

            return result;
        }
    }
}
