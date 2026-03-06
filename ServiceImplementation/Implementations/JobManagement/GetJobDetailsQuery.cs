using MediatR;
using ServiceContracts.DTOs.JobManagement;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record GetJobDetailsQuery(int Id, string? CurrentUserId = null) : IRequest<JobDetailsDto>;
}
