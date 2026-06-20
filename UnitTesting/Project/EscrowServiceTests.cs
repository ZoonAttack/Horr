using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Payment;
using ServiceImplementation.Implementations.Wallet;
using ServiceImplementation.Exceptions;
using UnitTesting;

namespace UnitTesting.Project
{
    public class EscrowServiceTests
    {
        private AppDbContext GetContext() => DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        [Fact]
        public async Task FundFixedContractAsync_SufficientBalance_ShouldSucceed()
        {
            // Arrange
            using var context = GetContext();
            var clientId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid().ToString();
            
            var contract = new Contract
            {
                Id = 1,
                ClientId = clientId.ToString(),
                FreelancerId = freelancerId,
                AgreedRate = 100.00m,
                Status = ContractStatus.Active
            };
            context.Contracts.Add(contract);

            var wallet = new WalletBalance
            {
                UserId = clientId.ToString(),
                BalanceEGP = 200.00m,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.WalletBalances.Add(wallet);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);

            // Act
            await escrowService.FundFixedContractAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"), clientId);

            // Assert
            var updatedWallet = await context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == clientId.ToString());
            updatedWallet!.BalanceEGP.Should().Be(94.50m); // 200 - (100 + 5.50 client fee)

            var escrowTx = await context.EscrowTransactions.FirstOrDefaultAsync(e => e.ContractId == contract.Id);
            escrowTx.Should().NotBeNull();
            escrowTx!.Amount.Should().Be(100.00m);
            escrowTx.PlatformFeeFromClient.Should().Be(5.50m);
            escrowTx.PlatformFeeFromFreelancer.Should().Be(15.00m);
            escrowTx.NetToFreelancer.Should().Be(85.00m);
            escrowTx.Status.Should().Be(EscrowStatus.Held);
            escrowTx.Type.Should().Be(EscrowTransactionType.ClientFunded);
        }

        [Fact]
        public async Task FundFixedContractAsync_InsufficientBalance_ShouldThrowException()
        {
            // Arrange
            using var context = GetContext();
            var clientId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid().ToString();

            var contract = new Contract
            {
                Id = 2,
                ClientId = clientId.ToString(),
                FreelancerId = freelancerId,
                AgreedRate = 100.00m,
                Status = ContractStatus.Active
            };
            context.Contracts.Add(contract);

            var wallet = new WalletBalance
            {
                UserId = clientId.ToString(),
                BalanceEGP = 50.00m,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.WalletBalances.Add(wallet);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);

            // Act
            var result = await escrowService.FundFixedContractAsync(Guid.Parse("00000000-0000-0000-0000-000000000002"), clientId);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Be("Insufficient wallet balance.");
        }

        [Fact]
        public async Task FundMilestoneAsync_SufficientBalance_ShouldSucceed()
        {
            // Arrange
            using var context = GetContext();
            var clientId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid().ToString();

            var contract = new Contract
            {
                Id = 3,
                ClientId = clientId.ToString(),
                FreelancerId = freelancerId,
                Status = ContractStatus.Active
            };
            context.Contracts.Add(contract);

            var milestone = new ContractMilestone
            {
                Id = Guid.NewGuid(),
                ContractId = 3,
                Title = "Milestone 1",
                Amount = 100.00m,
                Status = MilestoneStatus.Unfunded
            };
            context.ContractMilestones.Add(milestone);

            var wallet = new WalletBalance
            {
                UserId = clientId.ToString(),
                BalanceEGP = 150.00m,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.WalletBalances.Add(wallet);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);

            // Act
            await escrowService.FundMilestoneAsync(milestone.Id, clientId);

            // Assert
            var updatedWallet = await context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == clientId.ToString());
            updatedWallet!.BalanceEGP.Should().Be(44.50m); // 150 - 105.50

            var updatedMilestone = await context.ContractMilestones.FirstOrDefaultAsync(m => m.Id == milestone.Id);
            updatedMilestone!.Status.Should().Be(MilestoneStatus.Funded);
            updatedMilestone.FundedAt.Should().NotBeNull();

            var escrowTx = await context.EscrowTransactions.FirstOrDefaultAsync(e => e.ContractMilestoneId == milestone.Id);
            escrowTx.Should().NotBeNull();
            escrowTx!.Amount.Should().Be(100.00m);
            escrowTx.Status.Should().Be(EscrowStatus.Held);
        }

        [Fact]
        public async Task ReleaseToFreelancerAsync_ShouldCreditFreelancer_AndUpdateStatus()
        {
            // Arrange
            using var context = GetContext();
            var clientId = Guid.NewGuid().ToString();
            var freelancerId = Guid.NewGuid().ToString();

            var contract = new Contract
            {
                Id = 4,
                ClientId = clientId,
                FreelancerId = freelancerId,
                Status = ContractStatus.Active
            };
            context.Contracts.Add(contract);

            var escrowTx = new EscrowTransaction
            {
                ContractId = 4,
                ContractMilestoneId = null,
                Type = EscrowTransactionType.ClientFunded,
                Amount = 100.00m,
                PlatformFeeFromClient = 5.50m,
                PlatformFeeFromFreelancer = 15.00m,
                NetToFreelancer = 85.00m,
                Status = EscrowStatus.Held
            };
            context.EscrowTransactions.Add(escrowTx);

            var wallet = new WalletBalance
            {
                UserId = freelancerId,
                BalanceEGP = 10.00m,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.WalletBalances.Add(wallet);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);

            // Act
            await escrowService.ReleaseToFreelancerAsync(Guid.Parse("00000000-0000-0000-0000-000000000004"), null);

            // Assert
            var updatedWallet = await context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == freelancerId);
            updatedWallet!.BalanceEGP.Should().Be(95.00m); // 10 + 85 net payment

            var originalTx = await context.EscrowTransactions.FirstOrDefaultAsync(e => e.Id == escrowTx.Id);
            originalTx!.Status.Should().Be(EscrowStatus.Released);

            var payoutTxExists = await context.EscrowTransactions.AnyAsync(e => e.ContractId == contract.Id && e.Type == EscrowTransactionType.ReleasedToFreelancer);
            payoutTxExists.Should().BeTrue();
        }

        [Fact]
        public async Task RefundToClientAsync_ShouldRefundClient_AndUpdateStatus()
        {
            // Arrange
            using var context = GetContext();
            var clientId = Guid.NewGuid().ToString();
            var freelancerId = Guid.NewGuid().ToString();

            var contract = new Contract
            {
                Id = 5,
                ClientId = clientId,
                FreelancerId = freelancerId,
                Status = ContractStatus.Active
            };
            context.Contracts.Add(contract);

            var escrowTx = new EscrowTransaction
            {
                ContractId = 5,
                ContractMilestoneId = null,
                Type = EscrowTransactionType.ClientFunded,
                Amount = 100.00m,
                PlatformFeeFromClient = 5.50m,
                PlatformFeeFromFreelancer = 15.00m,
                NetToFreelancer = 85.00m,
                Status = EscrowStatus.Held
            };
            context.EscrowTransactions.Add(escrowTx);

            var wallet = new WalletBalance
            {
                UserId = clientId,
                BalanceEGP = 0.00m,
                LastUpdatedAt = DateTime.UtcNow
            };
            context.WalletBalances.Add(wallet);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);

            // Act
            await escrowService.RefundToClientAsync(Guid.Parse("00000000-0000-0000-0000-000000000005"), null, "Cancelled by Admin");

            // Assert
            var updatedWallet = await context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == clientId);
            updatedWallet!.BalanceEGP.Should().Be(100.00m); // original base amount refunded (100.00)

            var originalTx = await context.EscrowTransactions.FirstOrDefaultAsync(e => e.Id == escrowTx.Id);
            originalTx!.Status.Should().Be(EscrowStatus.Refunded);

            var refundTxExists = await context.EscrowTransactions.AnyAsync(e => e.ContractId == contract.Id && e.Type == EscrowTransactionType.RefundedToClient);
            refundTxExists.Should().BeTrue();
        }
    }
}
