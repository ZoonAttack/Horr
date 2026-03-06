using MediatR;
using ServiceContracts.DTOs.JobManagement;
using Entities.Enums;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record SearchJobsQuery(
        string? Keyword = null,
        JobType? JobType = null,
        JobSortEnum SortBy = JobSortEnum.Newest,
        int Page = 1,
        int PageSize = 10,
        string? CurrentUserId = null
    ) : IRequest<SearchJobsQueryResponse>;
}
