using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceImplementation.Exceptions;

using Entities.Project;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class ToggleSavedJobCommandHandler : IRequestHandler<ToggleSavedJobCommand, Result<bool>>
    {
        private readonly AppDbContext _context;

        public ToggleSavedJobCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ToggleSavedJobCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            var savedJob = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.JobPostId == request.JobPostId && s.FreelancerId == request.FreelancerId, cancellationToken);

            bool isSaved;
            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                isSaved = false;
            }
            else
            {
                // Verify job exists before saving
                var jobExists = await _context.JobPosts.AnyAsync(j => j.Id == request.JobPostId, cancellationToken);
                if (!jobExists)
                {
                    return new Result<bool>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCodes.JobNotFound,
                        Message = $"Job with ID {request.JobPostId} not found"
                    };
                }

                _context.SavedJobs.Add(new SavedJob
                {
                    JobPostId = request.JobPostId,
                    FreelancerId = request.FreelancerId,
                    SavedAt = DateTime.UtcNow
                });
                isSaved = true;

                // Automatically track the save interaction
                _context.Interactions.Add(new Entities.Users.Interactions
                {
                    UserId = request.FreelancerId,
                    TargetId = request.JobPostId,
                    TargetType = "job",
                    Action = Entities.Enums.InteractionTypes.Save,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new Result<bool> { Succeeded = true, Data = isSaved };
        }
    }
}
