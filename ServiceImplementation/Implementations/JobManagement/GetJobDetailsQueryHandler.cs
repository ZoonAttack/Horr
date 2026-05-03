using Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Mappings;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class GetJobDetailsQueryHandler : IRequestHandler<GetJobDetailsQuery, JobDetailsDto>
    {
        private readonly AppDbContext _context;

        public GetJobDetailsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobDetailsDto> Handle(GetJobDetailsQuery request, CancellationToken cancellationToken)
        {
            var jobQuery = _context.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.SavedByFreelancers);

            if (request.CurrentUserId != null)
            {
                // If user is owner, we might need more info for stats
                // But we can also do separate counts to keep the main entity load lean
            }

            var job = await jobQuery.FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

            if (job == null)
            {
                throw new NotFoundException($"Job with ID {request.Id} not found");
            }

            var dto = job.ToDetailsDto(request.CurrentUserId);

            // If the requester is the owner, populate stats
            if (request.CurrentUserId == job.ClientId)
            {
                dto.Stats = new JobStatsDto
                {
                    Proposals = await _context.Proposals.CountAsync(p => p.JobPostId == job.Id, cancellationToken),
                    Invited = await _context.JobInvitations.CountAsync(i => i.JobPostId == job.Id, cancellationToken),
                    Hired = await _context.Contracts.CountAsync(c => c.JobPostId == job.Id && 
                        c.Status != Entities.Enums.ContractStatus.Rejected && 
                        c.Status != Entities.Enums.ContractStatus.Terminated, cancellationToken),
                    Messaged = 0 // Placeholder
                };
            }

            return dto;
        }
    }
}
