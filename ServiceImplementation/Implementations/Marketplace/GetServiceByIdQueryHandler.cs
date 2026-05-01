using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceCatalogItemDto>
    {
        private readonly AppDbContext _context;

        public GetServiceByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCatalogItemDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.GalleryFiles)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

            if (service == null)
            {
                throw new NotFoundException($"Service with ID {request.Id} not found.");
            }

            if (service.FreelancerId != request.FreelancerId)
            {
                // To the user, it should look like it doesn't exist or is inaccessible
                throw new NotFoundException($"Service with ID {request.Id} not found.");
            }

            return service.ToDto();
        }
    }
}
