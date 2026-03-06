using Entities.Enums;

namespace ServiceContracts.DTOs.JobManagement
{
    public class JobSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public string BudgetFormatted { get; set; } = string.Empty;
        public JobType JobType { get; set; }
        public DateTime PostedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public bool IsSaved { get; set; }
    }

    public class JobDetailsDto : JobSummaryDto
    {
        public string Description { get; set; } = string.Empty;
    }

    public class SearchJobsQueryResponse
    {
        public List<JobSummaryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
