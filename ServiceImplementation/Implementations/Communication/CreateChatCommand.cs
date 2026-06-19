using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public class CreateChatCommand : IRequest<Result<ChatSummaryDto>>
    {
        public int ContractId { get; }
        public string ClientId { get; }

        public CreateChatCommand(int contractId, string clientId)
        {
            ContractId = contractId;
            ClientId = clientId;
        }
    }
}
