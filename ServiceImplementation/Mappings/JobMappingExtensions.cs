using Entities.Project;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Mappings
{
    public static class JobMappingExtensions
    {
        public static JobSummaryDto ToSummaryDto(this JobPost job, string? currentUserId = null)
        {
            return new JobSummaryDto
            {
                Id = job.Id,
                Title = job.Title,
                CategoryId = job.CategoryId,
                CategoryName = job.Category?.Name ?? string.Empty,
                Scope = job.Scope,
                ExperienceLevel = job.ExperienceLevel,
                Budget = job.Budget,
                BudgetCurrency = job.BudgetCurrency,
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                ClientName = job.Client?.FullName ?? "Unknown",
                Skills = job.JobSkills?.Where(js => js.Skill != null).Select(js => js.Skill.Name).ToList() ?? new List<string>(),
                IsSaved = currentUserId != null && (job.SavedByFreelancers?.Any(s => s.FreelancerId == currentUserId) ?? false)
            };
        }

        public static JobDetailsDto ToDetailsDto(this JobPost job, string? currentUserId = null)
        {
            var dto = new JobDetailsDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                CategoryId = job.CategoryId,
                CategoryName = job.Category?.Name ?? string.Empty,
                Scope = job.Scope,
                ExperienceLevel = job.ExperienceLevel,
                Budget = job.Budget,
                BudgetCurrency = job.BudgetCurrency,
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                ClientName = job.Client?.FullName ?? "Unknown",
                Skills = job.JobSkills?.Where(js => js.Skill != null).Select(js => js.Skill.Name).ToList() ?? new List<string>(),
                IsSaved = currentUserId != null && (job.SavedByFreelancers?.Any(s => s.FreelancerId == currentUserId) ?? false)
            };

            return dto;
        }
    }
}
