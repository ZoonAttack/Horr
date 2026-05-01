using Entities;
using Entities.Users.FreelancerHelpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public class SaveFreelancerCommandHandler : IRequestHandler<SaveFreelancerCommand, bool>
    {
        private readonly AppDbContext _context;

        public SaveFreelancerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(SaveFreelancerCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.SavedFreelancers
                .AnyAsync(sf => sf.ClientId == request.ClientId && sf.FreelancerId == request.FreelancerId, cancellationToken);

            if (exists)
            {
                return true; // Already saved
            }

            var savedFreelancer = new SavedFreelancer
            {
                ClientId = request.ClientId,
                FreelancerId = request.FreelancerId,
                SavedAt = DateTime.UtcNow
            };

            _context.SavedFreelancers.Add(savedFreelancer);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }

    public class UnsaveFreelancerCommandHandler : IRequestHandler<UnsaveFreelancerCommand, bool>
    {
        private readonly AppDbContext _context;

        public UnsaveFreelancerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UnsaveFreelancerCommand request, CancellationToken cancellationToken)
        {
            var savedFreelancer = await _context.SavedFreelancers
                .FirstOrDefaultAsync(sf => sf.ClientId == request.ClientId && sf.FreelancerId == request.FreelancerId, cancellationToken);

            if (savedFreelancer == null)
            {
                return false;
            }

            _context.SavedFreelancers.Remove(savedFreelancer);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }
}
