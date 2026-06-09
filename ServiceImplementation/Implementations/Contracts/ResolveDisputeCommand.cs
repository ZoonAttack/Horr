using System;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public enum DisputeDecision
    {
        ForFreelancer,
        ForClient
    }

    public record ResolveDisputeCommand(
        Guid DisputeId,
        DisputeDecision Decision,
        string AdminDecision,
        string AdminId
    ) : IRequest<DisputeDto>;
}
