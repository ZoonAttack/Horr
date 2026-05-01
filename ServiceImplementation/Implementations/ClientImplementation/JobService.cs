using Entities;
using Entities.Project;
using Entities.Skill;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.ClientImplementation
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _db;

        public JobService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<JobDetailsDto>> CreateJobAsync(string clientId, JobDetailsDto jobDetails)
        {
            // Validate client exists properly
            var clientUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == clientId && u.Role == UserRole.Client);
            if (clientUser == null)
            {
                return new Result<JobDetailsDto>
                {
                    Succeeded = false,
                    Message = "Client not found or unauthenticated.",
                    Errors = { "Invalid client ID." }
                };
            }

            // Create new JobPost
            var jobPost = new JobPost
            {
                Id = Guid.NewGuid().ToString(),
                Title = jobDetails.Title,
                Description = jobDetails.Description,
                Category = jobDetails.Category,
                Scope = jobDetails.Scope,
                ExperienceLevel = jobDetails.ExperienceLevel,
                Budget = jobDetails.Budget,
                JobType = jobDetails.JobType,
                PostedAt = DateTime.UtcNow,
                ClientId = clientId,
                IsDeleted = false
            };

            // Map Skills
            // The frontend sends unique valid Skill.Id strings. It maps to JobSkill
            if (jobDetails.Skills != null && jobDetails.Skills.Any())
            {
                jobPost.JobSkills = jobDetails.Skills.Select(skillId => new JobSkill
                {
                    JobPostId = jobPost.Id,
                    SkillId = skillId
                }).ToList();
            }

            await _db.JobPosts.AddAsync(jobPost);
            await _db.SaveChangesAsync();

            // Return the created DTO with populated Id and generic fields
            jobDetails.Id = jobPost.Id;
            jobDetails.PostedAt = jobPost.PostedAt;
            jobDetails.ClientName = clientUser.FullName; 

            return new Result<JobDetailsDto>
            {
                Succeeded = true,
                Message = "Job created successfully.",
                Data = jobDetails
            };
        }

        public async Task<Result<List<JobSummaryDto>>> GetAllJobsAsync()
        {
            var jobs = await _db.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => !j.IsDeleted) // Global query filter usually handles this, but explicit doesn't hurt
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            var jobSummaries = jobs.Select(j => new JobSummaryDto
            {
                Id = j.Id,
                Title = j.Title,
                Category = j.Category,
                Scope = j.Scope,
                ExperienceLevel = j.ExperienceLevel,
                Budget = j.Budget,
                JobType = j.JobType,
                PostedAt = j.PostedAt,
                ClientName = j.Client?.FullName ?? "Unknown Client",
                Skills = j.JobSkills.Select(js => js.Skill?.Name ?? js.SkillId).ToList(),
                IsSaved = false // Current client API doesn't know context for "saved". This needs user context if extended.
            }).ToList();

            return new Result<List<JobSummaryDto>>
            {
                Succeeded = true,
                Message = "Jobs retrieved successfully.",
                Data = jobSummaries
            };
        }

        public async Task<Result<JobDetailsDto>> GetJobDetailsAsync(string jobId)
        {
            var job = await _db.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId && !j.IsDeleted);

            if (job == null)
            {
                return new Result<JobDetailsDto>
                {
                    Succeeded = false,
                    Message = "Job not found.",
                    Errors = { "Job with the specified ID does not exist." }
                };
            }

            var jobDetails = new JobDetailsDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Category = job.Category,
                Scope = job.Scope,
                ExperienceLevel = job.ExperienceLevel,
                Budget = job.Budget,
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                ClientName = job.Client?.FullName ?? "Unknown Client",
                Skills = job.JobSkills.Select(js => js.Skill?.Name ?? js.SkillId).ToList(),
                IsSaved = false
            };

            return new Result<JobDetailsDto>
            {
                Succeeded = true,
                Message = "Job details retrieved successfully.",
                Data = jobDetails
            };
        }
    }
}
