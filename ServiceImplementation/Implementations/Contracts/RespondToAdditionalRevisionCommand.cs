using System;
using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record RespondToAdditionalRevisionCommand(
        Guid RequestId,
        string FreelancerId,
        bool Accept
    ) : IRequest<Result<bool>>;
}