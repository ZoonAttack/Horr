using System;
using System.Threading.Tasks;

namespace Services.Wallet
{
    public interface IEscrowService
    {
        // Client funds a fixed-price contract (full amount locked in escrow)
        Task FundFixedContractAsync(Guid contractId, Guid clientId);

        // Client funds a specific milestone
        Task FundMilestoneAsync(Guid milestoneId, Guid clientId);

        // Release escrow to freelancer wallet after approval
        Task ReleaseToFreelancerAsync(Guid contractId, Guid? milestoneId);

        // Refund escrow to client wallet (admin decision or auto-cancel)
        Task RefundToClientAsync(Guid contractId, Guid? milestoneId, string reason);
    }
}
