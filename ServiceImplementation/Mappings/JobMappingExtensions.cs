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
                BudgetMin = job.BudgetMin,
                BudgetMax = job.BudgetMax,
                BudgetFormatted = CurrencyFormatter.ToEgpRange(job.BudgetMin, job.BudgetMax),
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                ClientName = job.Client?.FullName ?? "Unknown",
                Skills = job.JobSkills.Select(js => js.Skill.Name).ToList(),
                IsSaved = currentUserId != null && job.SavedByFreelancers.Any(s => s.FreelancerId == currentUserId)
            };
        }

        public static JobDetailsDto ToDetailsDto(this JobPost job, string? currentUserId = null)
        {
            var dto = new JobDetailsDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                BudgetMin = job.BudgetMin,
                BudgetMax = job.BudgetMax,
                BudgetFormatted = CurrencyFormatter.ToEgpRange(job.BudgetMin, job.BudgetMax),
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                ClientName = job.Client?.FullName ?? "Unknown",
                Skills = job.JobSkills.Select(js => js.Skill.Name).ToList(),
                IsSaved = currentUserId != null && job.SavedByFreelancers.Any(s => s.FreelancerId == currentUserId)
            };

            return dto;
        }
    }
}
