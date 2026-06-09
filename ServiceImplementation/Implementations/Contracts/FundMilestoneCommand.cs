using System;
using MediatR;

namespace ServiceImplementation.Implementations.Contracts
{
    public record FundMilestoneCommand(
        Guid MilestoneId,
        Guid ClientId
    ) : IRequest<bool>;
}
