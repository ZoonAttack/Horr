using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record ToggleSavedJobCommand(string JobPostId, string FreelancerId) : IRequest<Result<bool>>;
}
