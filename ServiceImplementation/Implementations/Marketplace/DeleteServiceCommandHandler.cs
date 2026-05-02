using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceImplementation.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand>
    {
        private readonly AppDbContext _context;

        public DeleteServiceCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceCatalogItems
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (service == null)
            {
                throw new NotFoundException($"Service with ID {request.Id} not found.");
            }

            if (service.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You do not own this service.");
            }

            service.IsDeleted = true;
            service.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
