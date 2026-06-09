using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class RevokeOfferCommand : IRequest<Result<bool>>
    {
        public int ContractId { get; set; }
        public string ClientId { get; set; }

        public RevokeOfferCommand(int contractId, string clientId)
        {
            ContractId = contractId;
            ClientId = clientId;
        }
    }
}
