using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceImplementation.Exceptions;

using Entities.Project;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class ToggleSavedJobCommandHandler : IRequestHandler<ToggleSavedJobCommand>
    {
        private readonly AppDbContext _context;

        public ToggleSavedJobCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleSavedJobCommand request, CancellationToken cancellationToken)
        {
            var savedJob = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.JobPostId == request.JobPostId && s.FreelancerId == request.FreelancerId, cancellationToken);

            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
            }
            else
            {
                // Verify job exists before saving
                var jobExists = await _context.JobPosts.AnyAsync(j => j.Id == request.JobPostId, cancellationToken);
                if (!jobExists)
                {
                    throw new NotFoundException($"Job with ID {request.JobPostId} not found");
                }

                _context.SavedJobs.Add(new SavedJob
                {
                    JobPostId = request.JobPostId,
                    FreelancerId = request.FreelancerId,
                    SavedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
