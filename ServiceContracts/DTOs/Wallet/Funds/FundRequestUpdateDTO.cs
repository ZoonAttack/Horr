using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.Funds
{
    // For Admin to approve/reject
    public class FundRequestUpdateDTO
    {
        public RequestStatus Status { get; set; }
        // Admin ID is captured in the service layer, not the DTO
    }
}
