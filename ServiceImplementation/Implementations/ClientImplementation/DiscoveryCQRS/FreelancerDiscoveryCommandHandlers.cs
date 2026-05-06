using Entities;
using Entities.Users.FreelancerHelpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public class SaveFreelancerCommandHandler : IRequestHandler<SaveFreelancerCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public SaveFreelancerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(SaveFreelancerCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }
            var exists = await _context.SavedFreelancers
                .AnyAsync(sf => sf.ClientId == request.ClientId && sf.FreelancerId == request.FreelancerId, cancellationToken);

            if (exists)
            {
                return new Result<bool> { Succeeded = true, Data = true }; // Already saved
            }

            var savedFreelancer = new SavedFreelancer
            {
                ClientId = request.ClientId,
                FreelancerId = request.FreelancerId,
                SavedAt = DateTime.UtcNow
            };

            _context.SavedFreelancers.Add(savedFreelancer);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = result > 0, Data = result > 0 };
        }
    }

    public class UnsaveFreelancerCommandHandler : IRequestHandler<UnsaveFreelancerCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public UnsaveFreelancerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UnsaveFreelancerCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }
            var savedFreelancer = await _context.SavedFreelancers
                .FirstOrDefaultAsync(sf => sf.ClientId == request.ClientId && sf.FreelancerId == request.FreelancerId, cancellationToken);

            if (savedFreelancer == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = "SAVED_FREELANCER_NOT_FOUND",
                    Message = "Saved freelancer not found."
                };
            }

            _context.SavedFreelancers.Remove(savedFreelancer);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = result > 0, Data = result > 0 };
        }
    }
}
