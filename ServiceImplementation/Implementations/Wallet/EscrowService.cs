using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Payment;
using Services.Wallet;
using ServiceImplementation.Exceptions;

namespace ServiceImplementation.Implementations.Wallet
{
    public class EscrowService : IEscrowService
    {
        private readonly AppDbContext _context;
        private readonly IWalletService _walletService;

        private const decimal CLIENT_SERVICE_FEE_PERCENT = 0.055m;
        private const decimal FREELANCER_COMMISSION_PERCENT = 0.15m;

        public EscrowService(AppDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        private int ConvertGuidToInt(Guid guid)
        {
            var guidString = guid.ToString();
            var parts = guidString.Split('-');
            var lastPart = parts[^1];
            if (int.TryParse(lastPart, System.Globalization.NumberStyles.HexNumber, null, out int hexInt))
            {
                return hexInt;
            }
            if (int.TryParse(guidString, out int directInt))
            {
                return directInt;
            }
            byte[] bytes = guid.ToByteArray();
            return Math.Abs(BitConverter.ToInt32(bytes, 0));
        }

        public async Task FundFixedContractAsync(Guid contractId, Guid clientId)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                throw new ValidationException("Contract not found.");
            }

            if (contract.ClientId != clientId.ToString())
            {
                throw new ValidationException("Contract does not belong to the specified client.");
            }

            decimal amount = contract.AgreedRate;
            decimal platformFeeFromClient = amount * CLIENT_SERVICE_FEE_PERCENT;
            decimal platformFeeFromFreelancer = amount * FREELANCER_COMMISSION_PERCENT;
            decimal netToFreelancer = amount - platformFeeFromFreelancer;
            decimal totalCharge = amount + platformFeeFromClient;

            // Debit Client Wallet
            await _walletService.DebitWalletAsync(
                clientId.ToString(),
                totalCharge,
                TransactionType.Escrow,
                $"Escrow funding for Contract #{contract.Id}"
            );

            // Create Escrow Transaction
            var escrowTx = new EscrowTransaction
            {
                ContractId = contract.Id,
                ContractMilestoneId = null,
                Type = EscrowTransactionType.ClientFunded,
                Amount = amount,
                PlatformFeeFromClient = platformFeeFromClient,
                PlatformFeeFromFreelancer = platformFeeFromFreelancer,
                NetToFreelancer = netToFreelancer,
                Status = EscrowStatus.Held,
                CreatedAt = DateTime.UtcNow
            };
            _context.EscrowTransactions.Add(escrowTx);

            await _context.SaveChangesAsync();
        }

        public async Task FundMilestoneAsync(Guid milestoneId, Guid clientId)
        {
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null)
            {
                throw new ValidationException("Milestone not found.");
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId);

            if (contract == null)
            {
                throw new ValidationException("Associated contract not found.");
            }

            if (contract.ClientId != clientId.ToString())
            {
                throw new ValidationException("Contract does not belong to the specified client.");
            }

            decimal amount = milestone.Amount;
            decimal platformFeeFromClient = amount * CLIENT_SERVICE_FEE_PERCENT;
            decimal platformFeeFromFreelancer = amount * FREELANCER_COMMISSION_PERCENT;
            decimal netToFreelancer = amount - platformFeeFromFreelancer;
            decimal totalCharge = amount + platformFeeFromClient;

            // Debit Client Wallet
            await _walletService.DebitWalletAsync(
                clientId.ToString(),
                totalCharge,
                TransactionType.Escrow,
                $"Escrow funding for Milestone: {milestone.Title}"
            );

            // Create Escrow Transaction
            var escrowTx = new EscrowTransaction
            {
                ContractId = contract.Id,
                ContractMilestoneId = milestone.Id,
                Type = EscrowTransactionType.ClientFunded,
                Amount = amount,
                PlatformFeeFromClient = platformFeeFromClient,
                PlatformFeeFromFreelancer = platformFeeFromFreelancer,
                NetToFreelancer = netToFreelancer,
                Status = EscrowStatus.Held,
                CreatedAt = DateTime.UtcNow
            };
            _context.EscrowTransactions.Add(escrowTx);

            milestone.Status = MilestoneStatus.Funded;
            milestone.FundedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ReleaseToFreelancerAsync(Guid contractId, Guid? milestoneId)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                throw new ValidationException("Contract not found.");
            }

            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == milestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded);

            if (escrowTx == null)
            {
                throw new ValidationException("No active escrow transaction found to release.");
            }

            // Credit Freelancer Wallet
            await _walletService.CreditWalletAsync(
                contract.FreelancerId,
                escrowTx.NetToFreelancer,
                TransactionType.Escrow,
                $"Escrow released for Contract #{contract.Id}"
            );

            // Update status to Released
            escrowTx.Status = EscrowStatus.Released;

            // Log payout audit trail EscrowTransaction
            var releaseTx = new EscrowTransaction
            {
                ContractId = contract.Id,
                ContractMilestoneId = milestoneId,
                Type = EscrowTransactionType.ReleasedToFreelancer,
                Amount = escrowTx.Amount,
                PlatformFeeFromClient = escrowTx.PlatformFeeFromClient,
                PlatformFeeFromFreelancer = escrowTx.PlatformFeeFromFreelancer,
                NetToFreelancer = escrowTx.NetToFreelancer,
                Status = EscrowStatus.Released,
                CreatedAt = DateTime.UtcNow
            };
            _context.EscrowTransactions.Add(releaseTx);

            if (milestoneId.HasValue)
            {
                var milestone = await _context.ContractMilestones
                    .FirstOrDefaultAsync(m => m.Id == milestoneId.Value);
                if (milestone != null)
                {
                    milestone.Status = MilestoneStatus.Released;
                    milestone.ReleasedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RefundToClientAsync(Guid contractId, Guid? milestoneId, string reason)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                throw new ValidationException("Contract not found.");
            }

            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == milestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded);

            if (escrowTx == null)
            {
                throw new ValidationException("No active escrow transaction found to refund.");
            }

            // Credit Client Wallet (base amount)
            await _walletService.CreditWalletAsync(
                contract.ClientId,
                escrowTx.Amount,
                TransactionType.Refund,
                $"Escrow refunded for Contract #{contract.Id}. Reason: {reason}"
            );

            // Update status to Refunded
            escrowTx.Status = EscrowStatus.Refunded;

            // Log refund audit trail EscrowTransaction
            var refundTx = new EscrowTransaction
            {
                ContractId = contract.Id,
                ContractMilestoneId = milestoneId,
                Type = EscrowTransactionType.RefundedToClient,
                Amount = escrowTx.Amount,
                PlatformFeeFromClient = 0,
                PlatformFeeFromFreelancer = 0,
                NetToFreelancer = 0,
                Status = EscrowStatus.Refunded,
                CreatedAt = DateTime.UtcNow
            };
            _context.EscrowTransactions.Add(refundTx);

            if (milestoneId.HasValue)
            {
                var milestone = await _context.ContractMilestones
                    .FirstOrDefaultAsync(m => m.Id == milestoneId.Value);
                if (milestone != null)
                {
                    milestone.Status = MilestoneStatus.Unfunded;
                    milestone.FundedAt = null;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
