using Entities.Enums;
using Entities.Project;
using ServiceImplementation.Exceptions;
using System.Linq;

namespace ServiceImplementation.Helpers
{
    public static class ContractStateGuard
    {
        // ── Delivery ──────────────────────────────────────────────────────────
        public static void EnsureCanDeliverWork(Contract contract)
        {
            if (contract.Status == ContractStatus.Closed ||
                contract.Status == ContractStatus.Completed ||
                contract.Status == ContractStatus.Terminated)
            {
                throw new InvalidStateException("Cannot deliver work on a closed contract.");
            }
        }

        // ── Accept Offer (Proposal must be Submitted) ─────────────────────────
        public static void EnsureCanAcceptOffer(Proposal proposal)
        {
            if (proposal.Status != ProposalStatus.Submitted)
            {
                throw new InvalidStateException("Only submitted proposals can be accepted.");
            }
        }

        // ── Decline Offer ──────────────────────────────────────────────────────
        public static void EnsureCanDeclineOffer(Proposal proposal)
        {
            if (proposal.Status != ProposalStatus.Submitted)
            {
                throw new InvalidStateException("Only submitted proposals can be declined.");
            }
        }

        // ── Reject Contract ────────────────────────────────────────────────────
        public static void EnsureCanRejectContract(Contract contract)
        {
            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.Active)
            {
                throw new InvalidStateException("Only draft or active contracts can be rejected.");
            }
        }

        // ── Complete Contract ──────────────────────────────────────────────────
        public static void EnsureCanComplete(Contract contract)
        {
            if (contract.Status != ContractStatus.Active)
            {
                throw new InvalidStateException("Only active contracts can be marked as completed.");
            }
        }

        // ── Review ────────────────────────────────────────────────────────────
        public static void EnsureCanSubmitReview(Contract contract, string reviewerId)
        {
            var hasDeliveries = (contract.WorkDeliveries != null && contract.WorkDeliveries.Any()) ||
                                (contract.ContractDeliveries != null && contract.ContractDeliveries.Any());

            if (!hasDeliveries)
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
