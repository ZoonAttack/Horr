using MediatR;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record GetJobDetailsQuery(string Id, string? CurrentUserId = null) : IRequest<Result<JobDetailsDto>>;
}
