using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetChatByContractQuery(int ContractId, string UserId) : IRequest<Result<ChatSummaryDto>>;
}
