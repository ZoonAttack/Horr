using MediatR;
using ServiceContracts.DTOs.JobManagement;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record GetJobDetailsQuery(string Id, string? CurrentUserId = null) : IRequest<JobDetailsDto>;
}
