using MediatR;

namespace ServiceImplementation.Implementations.JobManagement
{
    public record ToggleSavedJobCommand(int JobPostId, string FreelancerId) : IRequest;
}
