using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public class CreateChatCommand : IRequest<Result<ChatSummaryDto>>
    {
        public int ContractId { get; }
        public string RequestingUserId { get; }

        public CreateChatCommand(int contractId, string requestingUserId)
        {
            ContractId = contractId;
            RequestingUserId = requestingUserId;
        }
    }
}
