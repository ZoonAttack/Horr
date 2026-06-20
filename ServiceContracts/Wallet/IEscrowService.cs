using System;
using System.Threading.Tasks;
using ServiceContracts.DTOs.Responses;

namespace Services.Wallet
{
    public interface IEscrowService
    {
        // Client funds a fixed-price contract (full amount locked in escrow)
        Task<Result<bool>> FundFixedContractAsync(Guid contractId, Guid clientId);

        // Client funds a specific milestone
        Task<Result<bool>> FundMilestoneAsync(Guid milestoneId, Guid clientId);

        // Release escrow to freelancer wallet after approval
        Task<Result<bool>> ReleaseToFreelancerAsync(Guid contractId, Guid? milestoneId);

        // Refund escrow to client wallet (admin decision or auto-cancel)
        Task<Result<bool>> RefundToClientAsync(Guid contractId, Guid? milestoneId, string reason);

        // Resolve dispute with a percentage split between client and freelancer
        Task<Result<bool>> ResolveDisputeSplitAsync(Guid contractId, Guid? milestoneId, decimal clientPercentage, decimal freelancerPercentage, string reason);
    }
}
