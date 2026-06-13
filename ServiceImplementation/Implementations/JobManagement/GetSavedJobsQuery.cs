using MediatR;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record GetSavedJobsQuery(
        string FreelancerId,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<SearchJobsQueryResponse>>;
}
