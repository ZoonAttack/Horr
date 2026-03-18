using Entities.Enums;
using Entities.Project;
using ServiceImplementation.Exceptions;
using System.Linq;

namespace ServiceImplementation.Helpers
{
    public static class ContractStateGuard
    {
        public static void EnsureCanDeliverWork(Contract contract)
        {
            if (contract.Status == ContractStatus.Closed)
            {
                throw new InvalidStateException("Cannot deliver work on a closed contract.");
            }
        }

        public static void EnsureCanAcceptOffer(Proposal proposal)
        {
            if (proposal.Status != ProposalStatus.Offer)
            {
                throw new InvalidStateException("Only proposals with an 'Offer' status can be accepted.");
            }
        }

        public static void EnsureCanSubmitReview(Contract contract, string reviewerId)
        {
            if (contract.WorkDeliveries == null || !contract.WorkDeliveries.Any())
            {
                throw new InvalidStateException("Cannot review a contract with no delivered work.");
            }

            if (contract.ContractReviews != null && contract.ContractReviews.Any(r => r.ReviewerId == reviewerId))
            {
                throw new ConflictException("You have already reviewed this contract.");
            }
        }
    }
}
