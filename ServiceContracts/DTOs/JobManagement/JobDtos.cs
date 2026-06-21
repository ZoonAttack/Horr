using Entities.Enums;
using ServiceContracts.DTOs.Contract;

namespace ServiceContracts.DTOs.JobManagement
{
    public class JobStatsDto
    {
        public int Proposals { get; set; }
        public int Messaged { get; set; }
        public int Invited { get; set; }
        public int Hired { get; set; }
    }

    public class ClientJobSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public JobStatsDto Stats { get; set; } = new();
    }

    public class JobSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public ProjectComplexity Scope { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
        public decimal Budget { get; set; }
        public string BudgetCurrency { get; set; } = "USD";
        public decimal? ConvertedBudget { get; set; }
        public string? ConvertedCurrency { get; set; }
        public JobType JobType { get; set; }
        public DateTime PostedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public bool IsSaved { get; set; }
    }

    public class JobDetailsDto : JobSummaryDto
    {
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Milestones submitted by the client when posting the job.
        /// Each milestone represents a deliverable with a title, amount, and due date.
        /// </summary>
        public List<ContractMilestoneDto> Milestones { get; set; } = new();

        public JobStatsDto? Stats { get; set; }
        public bool HasApplied { get; set; }
    }

    public class SearchJobsQueryResponse
    {
        public List<JobSummaryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
