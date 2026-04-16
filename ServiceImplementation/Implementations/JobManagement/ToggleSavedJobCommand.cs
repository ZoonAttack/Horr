using MediatR;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record ToggleSavedJobCommand(string JobPostId, string FreelancerId) : IRequest;
}
