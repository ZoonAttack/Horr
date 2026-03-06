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
            var job = await _context.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.SavedByFreelancers)
                .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

            if (job == null)
            {
                throw new NotFoundException($"Job with ID {request.Id} not found");
            }

            return job.ToDetailsDto(request.CurrentUserId);
        }
    }
}
