using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Payment;
using Services.Wallet;
using ServiceContracts.DTOs.Responses;
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

        public async Task<Result<bool>> FundFixedContractAsync(Guid contractId, Guid clientId)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Contract not found.", Errors = new List<string> { "Contract not found." } };
            }

            if (contract.ClientId != clientId.ToString())
            {
                return new Result<bool> { Succeeded = false, Message = "Contract does not belong to the specified client.", Errors = new List<string> { "Contract does not belong to the specified client." } };
            }

            decimal amount = contract.AgreedRate;
            decimal platformFeeFromClient = amount * CLIENT_SERVICE_FEE_PERCENT;
            decimal platformFeeFromFreelancer = amount * FREELANCER_COMMISSION_PERCENT;
            decimal netToFreelancer = amount - platformFeeFromFreelancer;
            decimal totalCharge = amount + platformFeeFromClient;

            // Debit Client Wallet
            try
            {
                await _walletService.DebitWalletAsync(
                    clientId.ToString(),
                    totalCharge,
                    TransactionType.Escrow,
                    $"Escrow funding for Contract #{contract.Id}"
                );
            }
            catch (Exception ex)
            {
                return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
            }

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
            return new Result<bool> { Succeeded = true, Data = true };
        }

        public async Task<Result<bool>> FundMilestoneAsync(Guid milestoneId, Guid clientId)
        {
            var milestone = await _context.ContractMilestones
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Milestone not found.", Errors = new List<string> { "Milestone not found." } };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId);

            if (contract == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Associated contract not found.", Errors = new List<string> { "Associated contract not found." } };
            }

            if (contract.ClientId != clientId.ToString())
            {
                return new Result<bool> { Succeeded = false, Message = "Contract does not belong to the specified client.", Errors = new List<string> { "Contract does not belong to the specified client." } };
            }

            decimal amount = milestone.Amount;
            decimal platformFeeFromClient = amount * CLIENT_SERVICE_FEE_PERCENT;
            decimal platformFeeFromFreelancer = amount * FREELANCER_COMMISSION_PERCENT;
            decimal netToFreelancer = amount - platformFeeFromFreelancer;
            decimal totalCharge = amount + platformFeeFromClient;

            // Debit Client Wallet
            try
            {
                await _walletService.DebitWalletAsync(
                    clientId.ToString(),
                    totalCharge,
                    TransactionType.Escrow,
                    $"Escrow funding for Milestone: {milestone.Title}"
                );
            }
            catch (Exception ex)
            {
                return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
            }

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
            return new Result<bool> { Succeeded = true, Data = true };
        }

        public async Task<Result<bool>> ReleaseToFreelancerAsync(Guid contractId, Guid? milestoneId)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Contract not found.", Errors = new List<string> { "Contract not found." } };
            }

            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == milestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded);

            if (escrowTx == null)
            {
                return new Result<bool> { Succeeded = false, Message = "No active escrow transaction found to release.", Errors = new List<string> { "No active escrow transaction found to release." } };
            }

            // Credit Freelancer Wallet
            try
            {
                await _walletService.CreditWalletAsync(
                    contract.FreelancerId,
                    escrowTx.NetToFreelancer,
                    TransactionType.Escrow,
                    $"Escrow released for Contract #{contract.Id}"
                );
            }
            catch (Exception ex)
            {
                return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
            }

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
            return new Result<bool> { Succeeded = true, Data = true };
        }

        public async Task<Result<bool>> RefundToClientAsync(Guid contractId, Guid? milestoneId, string reason)
        {
            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Contract not found.", Errors = new List<string> { "Contract not found." } };
            }

            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == milestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded);

            if (escrowTx == null)
            {
                return new Result<bool> { Succeeded = false, Message = "No active escrow transaction found to refund.", Errors = new List<string> { "No active escrow transaction found to refund." } };
            }

            // Credit Client Wallet (base amount)
            try
            {
                await _walletService.CreditWalletAsync(
                    contract.ClientId,
                    escrowTx.Amount,
                    TransactionType.Refund,
                    $"Escrow refunded for Contract #{contract.Id}. Reason: {reason}"
                );
            }
            catch (Exception ex)
            {
                return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
            }

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
            return new Result<bool> { Succeeded = true, Data = true };
        }

        public async Task<Result<bool>> ResolveDisputeSplitAsync(Guid contractId, Guid? milestoneId, decimal clientPercentage, decimal freelancerPercentage, string reason)
        {
            if (clientPercentage < 0 || clientPercentage > 100 || freelancerPercentage < 0 || freelancerPercentage > 100)
            {
                return new Result<bool> { Succeeded = false, Message = "Percentages must be between 0 and 100.", Errors = new List<string> { "Percentages must be between 0 and 100." } };
            }

            if (clientPercentage + freelancerPercentage != 100m)
            {
                return new Result<bool> { Succeeded = false, Message = "Percentages must sum to exactly 100.", Errors = new List<string> { "Percentages must sum to exactly 100." } };
            }

            int intContractId = ConvertGuidToInt(contractId);
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == intContractId);

            if (contract == null)
            {
                return new Result<bool> { Succeeded = false, Message = "Contract not found.", Errors = new List<string> { "Contract not found." } };
            }

            var escrowTx = await _context.EscrowTransactions
                .FirstOrDefaultAsync(e => e.ContractId == contract.Id 
                                          && e.ContractMilestoneId == milestoneId 
                                          && e.Status == EscrowStatus.Held 
                                          && e.Type == EscrowTransactionType.ClientFunded);

            if (escrowTx == null)
            {
                return new Result<bool> { Succeeded = false, Message = "No active escrow transaction found to resolve.", Errors = new List<string> { "No active escrow transaction found to resolve." } };
            }

            decimal clientAmount = escrowTx.Amount * (clientPercentage / 100m);
            decimal freelancerAmount = escrowTx.Amount * (freelancerPercentage / 100m);

            decimal freelancerFee = freelancerAmount * FREELANCER_COMMISSION_PERCENT;
            decimal netToFreelancer = freelancerAmount - freelancerFee;

            // Credit Client Wallet if any
            if (clientAmount > 0)
            {
                try
                {
                    await _walletService.CreditWalletAsync(
                        contract.ClientId,
                        clientAmount,
                        TransactionType.Refund,
                        $"Escrow refunded ({clientPercentage}%) for Contract #{contract.Id}. Reason: {reason}"
                    );
                }
                catch (Exception ex)
                {
                    return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
                }
            }

            // Credit Freelancer Wallet if any
            if (netToFreelancer > 0)
            {
                try
                {
                    await _walletService.CreditWalletAsync(
                        contract.FreelancerId,
                        netToFreelancer,
                        TransactionType.Escrow,
                        $"Escrow released ({freelancerPercentage}%) for Contract #{contract.Id}. Reason: {reason}"
                    );
                }
                catch (Exception ex)
                {
                    return new Result<bool> { Succeeded = false, Message = ex.Message, Errors = new List<string> { ex.Message } };
                }
            }

            // Update status of the original transaction
            if (clientPercentage == 100m)
            {
                escrowTx.Status = EscrowStatus.Refunded;
            }
            else if (freelancerPercentage == 100m)
            {
                escrowTx.Status = EscrowStatus.Released;
            }
            else
            {
                escrowTx.Status = EscrowStatus.Split;
            }
            escrowTx.ClientPercentage = clientPercentage;
            escrowTx.FreelancerPercentage = freelancerPercentage;

            // Log refund audit trail if client got any
            if (clientAmount > 0)
            {
                var refundTx = new EscrowTransaction
                {
                    ContractId = contract.Id,
                    ContractMilestoneId = milestoneId,
                    Type = EscrowTransactionType.RefundedToClient,
                    Amount = clientAmount,
                    PlatformFeeFromClient = 0,
                    PlatformFeeFromFreelancer = 0,
                    NetToFreelancer = 0,
                    Status = EscrowStatus.Refunded,
                    ClientPercentage = clientPercentage,
                    FreelancerPercentage = freelancerPercentage,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EscrowTransactions.Add(refundTx);
            }

            // Log release audit trail if freelancer got any
            if (freelancerAmount > 0)
            {
                var releaseTx = new EscrowTransaction
                {
                    ContractId = contract.Id,
                    ContractMilestoneId = milestoneId,
                    Type = EscrowTransactionType.ReleasedToFreelancer,
                    Amount = freelancerAmount,
                    PlatformFeeFromClient = 0,
                    PlatformFeeFromFreelancer = freelancerFee,
                    NetToFreelancer = netToFreelancer,
                    Status = EscrowStatus.Released,
                    ClientPercentage = clientPercentage,
                    FreelancerPercentage = freelancerPercentage,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EscrowTransactions.Add(releaseTx);
            }

            if (milestoneId.HasValue)
            {
                var milestone = await _context.ContractMilestones
                    .FirstOrDefaultAsync(m => m.Id == milestoneId.Value);
                if (milestone != null)
                {
                    if (freelancerPercentage > 0)
                    {
                        milestone.Status = MilestoneStatus.Released;
                        milestone.ReleasedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        milestone.Status = MilestoneStatus.Unfunded;
                        milestone.FundedAt = null;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
