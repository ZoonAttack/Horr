using Entities.Enums;

namespace ServiceContracts.DTOs.Wallet.Withdrawls
{
    // For Admin to approve/reject
    public class WithdrawalRequestUpdateDTO
    {
        public RequestStatus Status { get; set; }
    }
}
