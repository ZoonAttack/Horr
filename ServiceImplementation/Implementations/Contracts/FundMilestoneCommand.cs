using System;
using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record FundMilestoneCommand(
        Guid MilestoneId,
        Guid ClientId
    ) : IRequest<Result<bool>>;
}
