using Entities;
using Entities.Project;
using Entities.Users;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.AI;
using ServiceContracts.Recommendations;
using ServiceContracts.DTOs.Recommendations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Recommendations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _db;
        private readonly IGeminiService _gemini;

        public RecommendationService(AppDbContext db, IGeminiService gemini)
        {
            _db = db;
            _gemini = gemini;
        }

        public async Task<List<RecommendedJobDTO>> GetRecommendedJobsForFreelancerAsync(string userId)
        {
            // 1. Load freelancer profile
            var freelancer = await _db.Freelancers
                .Include(f => f.User)
                .Include(f => f.FreelancerSkills).ThenInclude(fs => fs.Skill)
                .FirstOrDefaultAsync(f => f.UserId == userId);

            if (freelancer == null) return new List<RecommendedJobDTO>();

            var skillIds = freelancer.FreelancerSkills.Select(fs => fs.SkillId).ToList();
            var skillNames = freelancer.FreelancerSkills.Select(fs => fs.Skill.Name).ToList();

            // 2. Get excluded jobs (already applied)
            var appliedJobIds = await _db.Proposals
                .Where(p => p.FreelancerId == userId)
                .Select(p => p.JobPostId)
                .ToListAsync();

            // 3. Get recent interactions
            var recentInteractions = await _db.Interactions
                .Where(i => i.UserId == userId && i.TargetType == "job")
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new { i.TargetId, Action = i.Action.ToString() })
                .ToListAsync();

            // 4. Two-stage SQL Retrieval
            var candidateJobs = await _db.JobPosts
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => !j.IsDeleted && !appliedJobIds.Contains(j.Id))
                .Where(j => j.JobSkills.Any(js => skillIds.Contains(js.SkillId))) // Skill overlap!
                .OrderByDescending(j => j.PostedAt)
                .Take(40)
                .Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    description = j.Description.Length > 150 ? j.Description.Substring(0, 150) + "..." : j.Description,
                    category = j.Category != null ? j.Category.Name : "",
                    budget = j.Budget,
                    jobType = j.JobType.ToString(),
                    scope = j.Scope.ToString(),
                    experienceLevel = j.ExperienceLevel.ToString(),
                    skills = j.JobSkills.Select(js => js.Skill.Name).ToList()
                })
                .ToListAsync();

            if (!candidateJobs.Any())
            {
                // Fallback to recent jobs if no skill overlaps are found
                candidateJobs = await _db.JobPosts
                    .Include(j => j.Category)
                    .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                    .Where(j => !j.IsDeleted && !appliedJobIds.Contains(j.Id))
                    .OrderByDescending(j => j.PostedAt)
                    .Take(40)
                    .Select(j => new
                    {
                        id = j.Id,
                        title = j.Title,
                        description = j.Description.Length > 150 ? j.Description.Substring(0, 150) + "..." : j.Description,
                        category = j.Category != null ? j.Category.Name : "",
                        budget = j.Budget,
                        jobType = j.JobType.ToString(),
                        scope = j.Scope.ToString(),
                        experienceLevel = j.ExperienceLevel.ToString(),
                        skills = j.JobSkills.Select(js => js.Skill.Name).ToList()
                    })
                    .ToListAsync();
            }

            if (!candidateJobs.Any()) return new List<RecommendedJobDTO>();

            // 5. Build prompt
            var prompt = $@"
You are a job recommendation engine for a freelancing platform targeting the Egyptian market.

FREELANCER PROFILE:
- Skills: {string.Join(", ", skillNames)}
- Bio: {freelancer.User?.Bio ?? "Not provided"}
- Experience level: {freelancer.ExperienceLevel}
- Hourly rate: {freelancer.HourlyRate} EGP

RECENT BEHAVIOR (what they interacted with recently):
{JsonSerializer.Serialize(recentInteractions)}

CANDIDATE JOBS (choose from these):
{JsonSerializer.Serialize(candidateJobs)}

TASK:
Select the 5 best job IDs for this freelancer.
Prioritize: skill match first, then budget fit, then description relevance.
If fewer than 5 are a good match, return fewer.

RESPOND with ONLY a JSON array of job ID strings.
Example format: [""abc123"",""def456""]
No explanation. Just the array.
";

            try
            {
                var raw = await _gemini.AskAsync(prompt);
                var recommendedIds = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();

                // 6. Fetch full job details
                var recommendedJobs = await _db.JobPosts
                    .Include(j => j.Category)
                    .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                    .Include(j => j.SavedByFreelancers)
                    .Where(j => recommendedIds.Contains(j.Id))
                    .ToListAsync();

                // Order by recommended index and map to DTO
                var result = recommendedJobs
                    .OrderBy(j => recommendedIds.IndexOf(j.Id))
                    .Select(j => new RecommendedJobDTO
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Description = j.Description,
                        Category = j.Category?.Name ?? string.Empty,
                        Budget = j.Budget,
                        JobType = j.JobType.ToString(),
                        Scope = j.Scope.ToString(),
                        ExperienceLevel = j.ExperienceLevel.ToString(),
                        PostedAt = j.PostedAt,
                        Skills = j.JobSkills.Select(js => js.Skill.Name).ToList(),
                        IsSaved = j.SavedByFreelancers.Any(s => s.FreelancerId == userId)
                    })
                    .ToList();

                return result;
            }
            catch
            {
                // Fallback to top candidate jobs mapped to DTO
                return candidateJobs.Take(5).Select(j => new RecommendedJobDTO
                {
                    Id = j.id,
                    Title = j.title,
                    Description = j.description,
                    Category = j.category,
                    Budget = j.budget,
                    JobType = j.jobType,
                    Scope = j.scope,
                    ExperienceLevel = j.experienceLevel,
                    PostedAt = DateTime.UtcNow,
                    Skills = j.skills,
                    IsSaved = false
                }).ToList();
            }
        }

        public async Task<List<RecommendedFreelancerDTO>> GetRecommendedFreelancersForClientAsync(string userId)
        {
            // 1. Get client's recent jobs
            var clientJobs = await _db.JobPosts
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.ClientId == userId && !j.IsDeleted)
                .OrderByDescending(j => j.PostedAt)
                .Take(5)
                .Select(j => new
                {
                    title = j.Title,
                    category = j.Category != null ? j.Category.Name : "",
                    budget = j.Budget,
                    skills = j.JobSkills.Select(js => js.Skill.Name).ToList()
                })
                .ToListAsync();

            // 2. Exclude hired
            var alreadyHiredIds = await _db.Contracts
                .Where(c => c.ClientId == userId)
                .Select(c => c.FreelancerId)
                .Distinct()
                .ToListAsync();

            // 3. Get recent interactions
            var recentInteractions = await _db.Interactions
                .Where(i => i.UserId == userId && i.TargetType == "freelancer")
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new { i.TargetId, Action = i.Action.ToString() })
                .ToListAsync();

            // 4. Pre-filtering Freelancers: Skill overlapping with client needs if client jobs exist
            var clientSkillsNeeded = clientJobs.SelectMany(j => j.skills).Distinct().ToList();

            IQueryable<Freelancer> freelancerQuery = _db.Freelancers
                .Include(f => f.User)
                .Include(f => f.FreelancerSkills).ThenInclude(fs => fs.Skill)
                .Where(f => !alreadyHiredIds.Contains(f.UserId));

            if (clientSkillsNeeded.Any())
            {
                // Skill pre-filter
                freelancerQuery = freelancerQuery.Where(f => f.FreelancerSkills.Any(fs => clientSkillsNeeded.Contains(fs.Skill.Name)));
            }

            var candidateFreelancers = await freelancerQuery
                .Take(40)
                .Select(f => new
                {
                    id = f.UserId,
                    name = f.User != null ? f.User.FullName : "",
                    title = f.Title ?? "",
                    bio = f.User != null && f.User.Bio != null && f.User.Bio.Length > 150
                        ? f.User.Bio.Substring(0, 150) + "..."
                        : (f.User != null ? f.User.Bio ?? "" : ""),
                    skills = f.FreelancerSkills.Select(fs => fs.Skill.Name).ToList(),
                    hourlyRate = f.HourlyRate ?? 0,
                    experienceLevel = f.ExperienceLevel.ToString()
                })
                .ToListAsync();

            if (!candidateFreelancers.Any())
            {
                // Fallback to top freelancers overall if no skill overlaps are found
                candidateFreelancers = await _db.Freelancers
                    .Include(f => f.User)
                    .Include(f => f.FreelancerSkills).ThenInclude(fs => fs.Skill)
                    .Where(f => !alreadyHiredIds.Contains(f.UserId))
                    .Take(40)
                    .Select(f => new
                    {
                        id = f.UserId,
                        name = f.User != null ? f.User.FullName : "",
                        title = f.Title ?? "",
                        bio = f.User != null && f.User.Bio != null && f.User.Bio.Length > 150
                            ? f.User.Bio.Substring(0, 150) + "..."
                            : (f.User != null ? f.User.Bio ?? "" : ""),
                        skills = f.FreelancerSkills.Select(fs => fs.Skill.Name).ToList(),
                        hourlyRate = f.HourlyRate ?? 0,
                        experienceLevel = f.ExperienceLevel.ToString()
                    })
                    .ToListAsync();
            }

            if (!candidateFreelancers.Any()) return new List<RecommendedFreelancerDTO>();

            // 5. Build prompt
            var clientContext = clientJobs.Any()
                ? $"Client's recent job posts: {JsonSerializer.Serialize(clientJobs)}"
                : "Client has not posted jobs yet. Use their interaction history as context.";

            var prompt = $@"
You are a freelancer recommendation engine for a freelancing platform targeting the Egyptian market.

CLIENT CONTEXT:
{clientContext}

RECENT BEHAVIOR (freelancers they viewed/saved recently):
{JsonSerializer.Serialize(recentInteractions)}

CANDIDATE FREELANCERS (choose from these):
{JsonSerializer.Serialize(candidateFreelancers)}

TASK:
Select the 5 best freelancer IDs for this client based on their needs.
Prioritize: skill match with their job posts, budget fit with their typical budgets, experience level.
If fewer than 5 are a good match, return fewer.

RESPOND with ONLY a JSON array of freelancer ID strings.
Example format: [""abc123"",""def456""]
No explanation. Just the array.
";

            try
            {
                var raw = await _gemini.AskAsync(prompt);
                var recommendedIds = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();

                var recommendedFreelancers = await _db.Freelancers
                    .Include(f => f.User)
                    .Include(f => f.FreelancerSkills).ThenInclude(fs => fs.Skill)
                    .Where(f => recommendedIds.Contains(f.UserId))
                    .ToListAsync();

                var result = recommendedFreelancers
                    .OrderBy(f => recommendedIds.IndexOf(f.UserId))
                    .Select(f => new RecommendedFreelancerDTO
                    {
                        UserId = f.UserId,
                        Title = f.Title ?? string.Empty,
                        Bio = f.User?.Bio ?? string.Empty,
                        HourlyRate = f.HourlyRate ?? 0,
                        ExperienceLevel = f.ExperienceLevel.ToString(),
                        Availability = !string.IsNullOrEmpty(f.Availability) && !f.Availability.Equals("not available", StringComparison.OrdinalIgnoreCase),
                        Name = f.User?.FullName ?? string.Empty,
                        Skills = f.FreelancerSkills.Select(fs => fs.Skill.Name).ToList()
                    })
                    .ToList();

                return result;
            }
            catch
            {
                // Fallback to top candidate freelancers mapped to DTO
                return candidateFreelancers.Take(5).Select(f => new RecommendedFreelancerDTO
                {
                    UserId = f.id,
                    Title = f.title,
                    Bio = f.bio,
                    HourlyRate = f.hourlyRate,
                    ExperienceLevel = f.experienceLevel,
                    Availability = true,
                    Name = f.name,
                    Skills = f.skills
                }).ToList();
            }
        }

        public async Task TrackInteractionAsync(string userId, TrackInteractionDTO interactionDto)
        {
            var interaction = new Interactions
            {
                UserId = userId,
                TargetId = interactionDto.TargetId,
                TargetType = interactionDto.TargetType,
                Action = interactionDto.Action,
                CreatedAt = DateTime.UtcNow
            };

            _db.Interactions.Add(interaction);
            await _db.SaveChangesAsync();
        }
    }
}
