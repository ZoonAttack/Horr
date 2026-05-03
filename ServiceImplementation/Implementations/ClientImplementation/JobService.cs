using Entities;
using Entities.Project;
using Entities.Skill;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
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
            var clientUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == clientId && u.Role == UserRole.Client);
            if (clientUser == null)
            {
                return new Result<JobDetailsDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ClientNotFound,
                    Message = "Client not found or unauthenticated.",
                    Errors = new List<string> { "Invalid client ID." }
                };
            }

            var jobPost = new JobPost
            {
                Id              = Guid.NewGuid().ToString(),
                Title           = jobDetails.Title,
                Description     = jobDetails.Description,
                Category        = jobDetails.Category,
                Scope           = jobDetails.Scope,
                ExperienceLevel = jobDetails.ExperienceLevel,
                Budget          = jobDetails.Budget,
                JobType         = jobDetails.JobType,
                PostedAt        = DateTime.UtcNow,
                ClientId        = clientId,
                IsDeleted       = false
            };

            // Map Skills
            if (jobDetails.Skills != null && jobDetails.Skills.Any())
            {
                jobPost.JobSkills = jobDetails.Skills.Select(skillId => new JobSkill
                {
                    JobPostId = jobPost.Id,
                    SkillId   = skillId
                }).ToList();
            }

            // Map Milestones
            if (jobDetails.Milestones != null && jobDetails.Milestones.Any())
            {
                jobPost.JobMilestones = jobDetails.Milestones.Select(m => new JobMilestone
                {
                    JobPostId = jobPost.Id,
                    Title     = m.Title,
                    Amount    = m.Amount,
                    DueDate   = m.DueDate
                }).ToList();
            }

            await _db.JobPosts.AddAsync(jobPost);
            await _db.SaveChangesAsync();

            jobDetails.Id         = jobPost.Id;
            jobDetails.PostedAt   = jobPost.PostedAt;
            jobDetails.ClientName = clientUser.FullName;

            return new Result<JobDetailsDto>
            {
                Succeeded = true,
                Message   = "Job created successfully.",
                Data      = jobDetails
            };
        }

        public async Task<Result<List<JobSummaryDto>>> GetAllJobsAsync()
        {
            var jobs = await _db.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => !j.IsDeleted)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            var jobSummaries = jobs.Select(j => new JobSummaryDto
            {
                Id              = j.Id,
                Title           = j.Title,
                Category        = j.Category,
                Scope           = j.Scope,
                ExperienceLevel = j.ExperienceLevel,
                Budget          = j.Budget,
                JobType         = j.JobType,
                PostedAt        = j.PostedAt,
                ClientName      = j.Client?.FullName ?? "Unknown Client",
                Skills          = j.JobSkills.Select(js => js.Skill?.Name ?? js.SkillId).ToList(),
                IsSaved         = false
            }).ToList();

            return new Result<List<JobSummaryDto>>
            {
                Succeeded = true,
                Message   = "Jobs retrieved successfully.",
                Data      = jobSummaries
            };
        }

        public async Task<Result<JobDetailsDto>> GetJobDetailsAsync(string jobId)
        {
            var job = await _db.JobPosts
                .Include(j => j.Client)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.JobMilestones)
                .FirstOrDefaultAsync(j => j.Id == jobId && !j.IsDeleted);

            if (job == null)
            {
                return new Result<JobDetailsDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.JobNotFound,
                    Message   = "Job not found.",
                    Errors    = new List<string> { "Job with the specified ID does not exist." }
                };
            }

            var jobDetails = new JobDetailsDto
            {
                Id              = job.Id,
                Title           = job.Title,
                Description     = job.Description,
                Category        = job.Category,
                Scope           = job.Scope,
                ExperienceLevel = job.ExperienceLevel,
                Budget          = job.Budget,
                JobType         = job.JobType,
                PostedAt        = job.PostedAt,
                ClientName      = job.Client?.FullName ?? "Unknown Client",
                Skills          = job.JobSkills.Select(js => js.Skill?.Name ?? js.SkillId).ToList(),
                IsSaved         = false,
                Milestones      = job.JobMilestones.Select(m => new ServiceContracts.DTOs.Contract.ContractMilestoneDto
                {
                    Title   = m.Title,
                    Amount  = m.Amount,
                    DueDate = m.DueDate
                }).ToList()
            };

            return new Result<JobDetailsDto>
            {
                Succeeded = true,
                Message   = "Job details retrieved successfully.",
                Data      = jobDetails
            };
        }
        public async Task<Result<List<ClientJobSummaryDto>>> GetClientJobsAsync(string clientId)
        {
            var jobs = await _db.JobPosts
                .Where(j => j.ClientId == clientId && !j.IsDeleted)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            var jobIds = jobs.Select(j => j.Id).ToList();
            
            // Batch load stats to avoid N+1
            var proposalCounts = await _db.Proposals
                .Where(p => jobIds.Contains(p.JobPostId))
                .GroupBy(p => p.JobPostId)
                .Select(g => new { JobPostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.JobPostId, x => x.Count);

            var hiredCounts = await _db.Contracts
                .Where(c => jobIds.Contains(c.JobPostId ?? "") && 
                           c.Status != ContractStatus.Rejected && 
                           c.Status != ContractStatus.Terminated)
                .GroupBy(c => c.JobPostId)
                .Select(g => new { JobPostId = g.Key!, Count = g.Count() })
                .ToDictionaryAsync(x => x.JobPostId, x => x.Count);

            var invitedCounts = await _db.JobInvitations
                .Where(i => jobIds.Contains(i.JobPostId))
                .GroupBy(i => i.JobPostId)
                .Select(g => new { JobPostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.JobPostId, x => x.Count);

            var result = jobs.Select(j => new ClientJobSummaryDto
            {
                Id = j.Id,
                Title = j.Title,
                PostedAt = j.PostedAt,
                Stats = new JobStatsDto
                {
                    Proposals = proposalCounts.GetValueOrDefault(j.Id),
                    Invited = invitedCounts.GetValueOrDefault(j.Id),
                    Hired = hiredCounts.GetValueOrDefault(j.Id),
                    Messaged = 0 // Placeholder until Chat/Job link is implemented
                }
            }).ToList();

            return new Result<List<ClientJobSummaryDto>>
            {
                Succeeded = true,
                Message = "Client jobs retrieved successfully.",
                Data = result
            };
        }
    }
}
