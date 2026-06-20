using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Entities;
using Entities.Enums;
using Entities.Project;
using Entities.Payment;
using User = Entities.Users.User;
using Freelancer = Entities.Users.Freelancer;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Implementations.Wallet;
using ServiceImplementation.Implementations.Contracts;
using ServiceImplementation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Services.Wallet;
using UnitTesting;

namespace UnitTesting.Project
{
    public class Phase3Tests
    {
        private AppDbContext GetContext() => DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        private static Entities.Users.User CreateUser(string id, string email) => new Entities.Users.User
        {
            Id = id,
            Email = email,
            UserName = email,
            FullName = id
        };

        private static Contract CreateActiveContract(int id, string clientId, string freelancerId, decimal agreedRate) => new Contract
        {
            Id = id,
            ClientId = clientId,
            FreelancerId = freelancerId,
            AgreedRate = agreedRate,
            Status = ContractStatus.Active,
            MaxRevisions = 3
        };

        private static EscrowTransaction CreateEscrow(int contractId, decimal amount, Guid? milestoneId = null)
        {
            decimal clientFee = amount * 0.055m;
            decimal freelancerFee = amount * 0.15m;
            return new EscrowTransaction
            {
                ContractId = contractId,
                ContractMilestoneId = milestoneId,
                Type = EscrowTransactionType.ClientFunded,
                Amount = amount,
                PlatformFeeFromClient = clientFee,
                PlatformFeeFromFreelancer = freelancerFee,
                NetToFreelancer = amount - freelancerFee,
                Status = EscrowStatus.Held,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task SubmitDelivery_ShouldSucceed_AndStoreAttachments()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(1, "client1", "free1", 500m);
            var escrow = CreateEscrow(1, 500m);

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            await context.SaveChangesAsync();

            var handler = new SubmitDeliveryCommandHandler(context);
            var command = new SubmitDeliveryCommand(
                ContractId: 1,
                ContractMilestoneId: null,
                DeliveryNote: "Here is the completed work",
                FreelancerId: "free1",
                Attachments: new List<AttachmentDto>
                {
                    new AttachmentDto { Type = AttachmentType.File, FileName = "design.png", StoragePath = "/uploads/design.png" },
                    new AttachmentDto { Type = AttachmentType.Link, Url = "https://github.com/myrepo" }
                }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DeliveryStatus.Pending);
            result.DeliveryNote.Should().Be("Here is the completed work");
            result.ReviewDeadline.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), TimeSpan.FromSeconds(5));
            result.Attachments.Should().HaveCount(2);

            var fileAtt = result.Attachments.First(a => a.Type == AttachmentType.File);
            fileAtt.FileName.Should().Be("design.png");
            fileAtt.StoragePath.Should().Be("/uploads/design.png");

            var linkAtt = result.Attachments.First(a => a.Type == AttachmentType.Link);
            linkAtt.Url.Should().Be("https://github.com/myrepo");
        }

        [Fact]
        public async Task SubmitDelivery_ShouldThrowValidationException_IfEscrowNotHeld()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(2, "client1", "free1", 500m);

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new SubmitDeliveryCommandHandler(context);
            var command = new SubmitDeliveryCommand(
                ContractId: 2,
                ContractMilestoneId: null,
                DeliveryNote: "Work done",
                FreelancerId: "free1",
                Attachments: new List<AttachmentDto>()
            );

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task SubmitDelivery_ShouldThrowForbiddenException_IfContractNotActive()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(3, "client1", "free1", 500m);
            contract.Status = ContractStatus.Draft; // Not Active
            var escrow = CreateEscrow(3, 500m);

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            await context.SaveChangesAsync();

            var handler = new SubmitDeliveryCommandHandler(context);
            var command = new SubmitDeliveryCommand(
                ContractId: 3,
                ContractMilestoneId: null,
                DeliveryNote: "Work done",
                FreelancerId: "free1",
                Attachments: new List<AttachmentDto>()
            );

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task ApproveDelivery_ShouldSucceed_AndReleaseEscrow()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(4, "client1", "free1", 100m);
            var escrow = CreateEscrow(4, 100m);

            // Setup freelancer wallet
            context.WalletBalances.Add(new WalletBalance { UserId = "free1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 4,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new ApproveDeliveryCommandHandler(context, escrowService);
            var command = new ApproveDeliveryCommand(delivery.Id, "client1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Status.Should().Be(DeliveryStatus.Approved);
            result.Data.CompletedAt.Should().NotBeNull();

            // Verify escrow was released (amount is 100, NetToFreelancer is 85 per 15% commission)
            var freelancerWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "free1");
            freelancerWallet.BalanceEGP.Should().Be(85.00m);

            var releaseTx = await context.EscrowTransactions
                .FirstOrDefaultAsync(t => t.ContractId == 4 && t.Type == EscrowTransactionType.ReleasedToFreelancer);
            releaseTx.Should().NotBeNull();
            releaseTx!.Status.Should().Be(EscrowStatus.Released);
        }

        [Fact]
        public async Task RequestRevision_ShouldTransitionStatus_AndKeepEscrowHeld()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(5, "client1", "free1", 500m);
            var escrow = CreateEscrow(5, 500m);
            var delivery = new ContractDelivery
            {
                ContractId = 5,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            await context.SaveChangesAsync();

            var handler = new RequestRevisionCommandHandler(context);
            var command = new RequestRevisionCommand(delivery.Id, "client1", "Please fix typography");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Status.Should().Be(RevisionStatus.Pending);
            result.Data.Reason.Should().Be("Please fix typography");

            var updatedDelivery = await context.ContractDeliveries.FindAsync(delivery.Id);
            updatedDelivery!.Status.Should().Be(DeliveryStatus.RevisionRequested);

            var escrowDb = await context.EscrowTransactions.FindAsync(escrow.Id);
            escrowDb!.Status.Should().Be(EscrowStatus.Held); // Escrow untouched
        }

        [Fact]
        public async Task OpenDispute_ShouldTransitionStatus_AndPreventDuplicate()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(6, "client1", "free1", 500m);
            var escrow = CreateEscrow(6, 500m);
            var delivery = new ContractDelivery
            {
                ContractId = 6,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            await context.SaveChangesAsync();

            var handler = new OpenDisputeCommandHandler(context);
            var command = new OpenDisputeCommand(6, delivery.Id, "client1", "Quality is poor");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DisputeStatus.Open);
            result.Reason.Should().Be("Quality is poor");

            var updatedDelivery = await context.ContractDeliveries.FindAsync(delivery.Id);
            updatedDelivery!.Status.Should().Be(DeliveryStatus.Disputed);

            // Duplicate dispute guard
            await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task ResolveDispute_ForFreelancer_ShouldReleaseEscrow()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var admin = CreateUser("admin1", "admin@test.com");
            var contract = CreateActiveContract(7, "client1", "free1", 200m);
            var escrow = CreateEscrow(7, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "free1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 7,
                Status = DeliveryStatus.Disputed,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            var dispute = new Dispute
            {
                ContractId = 7,
                ContractDeliveryId = delivery.Id,
                OpenedByUserId = "client1",
                Reason = "Wrong delivery",
                Status = DisputeStatus.Open
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Users.Add(admin);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            context.Disputes.Add(dispute);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new ResolveDisputeCommandHandler(context, escrowService);
            var command = new ResolveDisputeCommand(dispute.Id, DisputeDecision.ForFreelancer, "Freelancer fulfilled specifications", "admin1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DisputeStatus.ResolvedForFreelancer);
            result.AdminDecision.Should().Be("Freelancer fulfilled specifications");

            // Verify freelancer received funds
            var freelancerWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "free1");
            freelancerWallet.BalanceEGP.Should().Be(170.00m); // 200 * 0.85
        }

        [Fact]
        public async Task ResolveDispute_ForClient_ShouldRefundClient()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var admin = CreateUser("admin1", "admin@test.com");
            var contract = CreateActiveContract(8, "client1", "free1", 200m);
            var escrow = CreateEscrow(8, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "client1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 8,
                Status = DeliveryStatus.Disputed,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            var dispute = new Dispute
            {
                ContractId = 8,
                ContractDeliveryId = delivery.Id,
                OpenedByUserId = "free1",
                Reason = "Did not pay",
                Status = DisputeStatus.Open
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Users.Add(admin);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            context.Disputes.Add(dispute);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new ResolveDisputeCommandHandler(context, escrowService);
            var command = new ResolveDisputeCommand(dispute.Id, DisputeDecision.ForClient, "Freelancer failed milestones", "admin1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DisputeStatus.ResolvedForClient);

            // Verify client was refunded
            var clientWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "client1");
            clientWallet.BalanceEGP.Should().Be(200.00m); // Base refund amount
        }

        [Fact]
        public async Task ResolveDispute_Split50_50_ShouldRefundAndReleaseEscrow()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var admin = CreateUser("admin1", "admin@test.com");
            
            var proposal = new Proposal
            {
                Id = 101,
                JobPostId = "job1",
                FreelancerId = "free1",
                CoverLetter = "Cover letter",
                BidRate = 200m,
                Status = ProposalStatus.Active
            };
            context.Proposals.Add(proposal);

            var contract = CreateActiveContract(9, "client1", "free1", 200m);
            contract.ProposalId = 101;
            var escrow = CreateEscrow(9, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "client1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });
            context.WalletBalances.Add(new WalletBalance { UserId = "free1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 9,
                Status = DeliveryStatus.Disputed,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            var dispute = new Dispute
            {
                ContractId = 9,
                ContractDeliveryId = delivery.Id,
                OpenedByUserId = "client1",
                Reason = "Dispute",
                Status = DisputeStatus.Open
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Users.Add(admin);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            context.Disputes.Add(dispute);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new ResolveDisputeCommandHandler(context, escrowService);

            // 50/50 split resolution
            var command = new ResolveDisputeCommand(
                dispute.Id,
                null,
                "50/50 split resolution",
                "admin1",
                50m,
                50m
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DisputeStatus.ResolvedSplit);
            result.ClientPercentage.Should().Be(50m);
            result.FreelancerPercentage.Should().Be(50m);

            // Verify wallet balances:
            // Client gets 50% of 200 = 100
            var clientWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "client1");
            clientWallet.BalanceEGP.Should().Be(100.00m);

            // Freelancer gets 50% of 200 = 100 minus 15% platform commission = 85
            var freelancerWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "free1");
            freelancerWallet.BalanceEGP.Should().Be(85.00m);

            // Verify contract is terminated
            var closedContract = await context.Contracts.FirstAsync(c => c.Id == 9);
            closedContract.Status.Should().Be(ContractStatus.Terminated);
            closedContract.ClosedAt.Should().NotBeNull();

            // Verify proposal is closed
            var closedProposal = await context.Proposals.FirstAsync(p => p.Id == 101);
            closedProposal.Status.Should().Be(ProposalStatus.Rejected);
        }

        [Fact]
        public async Task ResolveDispute_Percentages_100_0_ShouldRefundClient()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var admin = CreateUser("admin1", "admin@test.com");
            var contract = CreateActiveContract(10, "client1", "free1", 200m);
            var escrow = CreateEscrow(10, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "client1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });
            context.WalletBalances.Add(new WalletBalance { UserId = "free1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 10,
                Status = DeliveryStatus.Disputed,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };

            var dispute = new Dispute
            {
                ContractId = 10,
                ContractDeliveryId = delivery.Id,
                OpenedByUserId = "free1",
                Reason = "Dispute",
                Status = DisputeStatus.Open
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Users.Add(admin);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            context.Disputes.Add(dispute);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new ResolveDisputeCommandHandler(context, escrowService);

            // 100% Client / 0% Freelancer split resolution
            var command = new ResolveDisputeCommand(
                dispute.Id,
                null,
                "100/0 resolution",
                "admin1",
                100m,
                0m
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(DisputeStatus.ResolvedForClient);

            // Client gets 100% of 200 = 200
            var clientWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "client1");
            clientWallet.BalanceEGP.Should().Be(200.00m);

            // Freelancer gets 0
            var freelancerWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "free1");
            freelancerWallet.BalanceEGP.Should().Be(0m);
        }

        [Fact]
        public async Task FundMilestone_ShouldDebitClientWallet_AndCreateEscrowHeld()
        {
            // Arrange
            using var context = GetContext();
            var clientGuid = Guid.NewGuid();
            var client = CreateUser(clientGuid.ToString(), "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(9, client.Id, "free1", 500m);

            var milestone = new ContractMilestone
            {
                Id = Guid.NewGuid(),
                ContractId = 9,
                Title = "Milestone 1",
                Amount = 200m,
                Status = MilestoneStatus.Unfunded
            };

            // Set client wallet to 500
            context.WalletBalances.Add(new WalletBalance { UserId = client.Id, BalanceEGP = 500m, LastUpdatedAt = DateTime.UtcNow });

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.ContractMilestones.Add(milestone);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new FundMilestoneCommandHandler(context, escrowService);
            var command = new FundMilestoneCommand(milestone.Id, clientGuid);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();

            // Client wallet balance decreased by 200 + 5.5% client fee = 211.00 EGP
            var clientWallet = await context.WalletBalances.FirstAsync(w => w.UserId == client.Id);
            clientWallet.BalanceEGP.Should().Be(289.00m); // 500 - 211

            var milestoneDb = await context.ContractMilestones.FindAsync(milestone.Id);
            milestoneDb!.Status.Should().Be(MilestoneStatus.Funded);

            var escrowDb = await context.EscrowTransactions
                .FirstOrDefaultAsync(t => t.ContractMilestoneId == milestone.Id);
            escrowDb.Should().NotBeNull();
            escrowDb!.Status.Should().Be(EscrowStatus.Held);
            escrowDb.Amount.Should().Be(200m);
        }

        [Fact]
        public async Task FundMilestone_ShouldThrowValidationException_IfInsufficientBalance()
        {
            // Arrange
            using var context = GetContext();
            var clientGuid = Guid.NewGuid();
            var client = CreateUser(clientGuid.ToString(), "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(10, client.Id, "free1", 500m);

            var milestone = new ContractMilestone
            {
                Id = Guid.NewGuid(),
                ContractId = 10,
                Title = "Milestone 1",
                Amount = 200m,
                Status = MilestoneStatus.Unfunded
            };

            // Set client wallet to 50 EGP (insufficient for 211.00 EGP)
            context.WalletBalances.Add(new WalletBalance { UserId = client.Id, BalanceEGP = 50m, LastUpdatedAt = DateTime.UtcNow });

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.ContractMilestones.Add(milestone);
            await context.SaveChangesAsync();

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            var handler = new FundMilestoneCommandHandler(context, escrowService);
            var command = new FundMilestoneCommand(milestone.Id, clientGuid);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Message.Should().Be("Insufficient wallet balance.");
        }

        [Fact]
        public async Task DeliveryAutoCompleteService_ShouldAutoApprovePastDeadlineDeliveries()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = CreateActiveContract(11, "client1", "free1", 200m);
            var escrow = CreateEscrow(11, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "free1", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            var delivery = new ContractDelivery
            {
                ContractId = 11,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddHours(-1) // Overdue by 1 hour
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.Add(delivery);
            await context.SaveChangesAsync();

            // Mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);
            
            // Scope services resolver setup
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(AppDbContext)))
                .Returns(context);

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IEscrowService)))
                .Returns(escrowService);

            var logger = new Mock<ILogger<DeliveryAutoCompleteService>>().Object;
            var bgService = new DeliveryAutoCompleteService(serviceProviderMock.Object, logger);

            // Act
            await bgService.ProcessPendingDeliveriesAsync(CancellationToken.None);

            // Assert
            var deliveryDb = await context.ContractDeliveries.FindAsync(delivery.Id);
            deliveryDb!.Status.Should().Be(DeliveryStatus.Approved);

            var freelancerWallet = await context.WalletBalances.FirstAsync(w => w.UserId == "free1");
            freelancerWallet.BalanceEGP.Should().Be(170.00m); // 200 * 0.85 released
        }

        [Fact]
        public async Task DeliveryAutoCompleteService_ShouldNotAutoApproveIfBlocked()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client2", "client2@test.com");
            var freelancer = CreateUser("free2", "free2@test.com");
            var contract = CreateActiveContract(22, "client2", "free2", 200m);
            var escrow = CreateEscrow(22, 200m);

            context.WalletBalances.Add(new WalletBalance { UserId = "free2", BalanceEGP = 0, LastUpdatedAt = DateTime.UtcNow });

            // Create three deliveries that are past deadline but have active blockers
            var deliveryDispute = new ContractDelivery { ContractId = 22, Status = DeliveryStatus.Pending, ReviewDeadline = DateTime.UtcNow.AddHours(-1) };
            var deliveryRevision = new ContractDelivery { ContractId = 22, Status = DeliveryStatus.Pending, ReviewDeadline = DateTime.UtcNow.AddHours(-1) };
            var deliveryReview = new ContractDelivery { ContractId = 22, Status = DeliveryStatus.Pending, ReviewDeadline = DateTime.UtcNow.AddHours(-1) };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.EscrowTransactions.Add(escrow);
            context.ContractDeliveries.AddRange(deliveryDispute, deliveryRevision, deliveryReview);
            await context.SaveChangesAsync();

            // Add blockers
            context.Disputes.Add(new Dispute { ContractId = 22, ContractDeliveryId = deliveryDispute.Id, OpenedByUserId = "client2", Reason = "Disputed", Status = DisputeStatus.Open });
            context.RevisionRequests.Add(new RevisionRequest { DeliveryId = deliveryRevision.Id, RequestedByClientId = "client2", Reason = "Revision needed", Status = RevisionStatus.Pending });
            context.ContractSpecialistReviews.Add(new ContractSpecialistReview { DeliveryId = deliveryReview.Id, RequestedByClientId = "client2", RequirementsSummary = "Expected a doc", Status = SpecialistReviewStatus.InProgress });
            await context.SaveChangesAsync();

            // Mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(AppDbContext))).Returns(context);

            var walletService = new WalletService(context);
            var escrowService = new EscrowService(context, walletService);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IEscrowService))).Returns(escrowService);

            var logger = new Mock<ILogger<DeliveryAutoCompleteService>>().Object;
            var bgService = new DeliveryAutoCompleteService(serviceProviderMock.Object, logger);

            // Act
            await bgService.ProcessPendingDeliveriesAsync(CancellationToken.None);

            // Assert: Status should remain Pending
            var dbDispute = await context.ContractDeliveries.FindAsync(deliveryDispute.Id);
            var dbRevision = await context.ContractDeliveries.FindAsync(deliveryRevision.Id);
            var dbReview = await context.ContractDeliveries.FindAsync(deliveryReview.Id);

            dbDispute!.Status.Should().Be(DeliveryStatus.Pending);
            dbRevision!.Status.Should().Be(DeliveryStatus.Pending);
            dbReview!.Status.Should().Be(DeliveryStatus.Pending);

            // Assert DTO mappings have correct properties
            var dtoDispute = ServiceContracts.DTOs.Contract.ContractDeliveryExtensions.ToDto(dbDispute);
            var dtoRevision = ServiceContracts.DTOs.Contract.ContractDeliveryExtensions.ToDto(dbRevision);
            var dtoReview = ServiceContracts.DTOs.Contract.ContractDeliveryExtensions.ToDto(dbReview);

            dtoDispute.IsPaused.Should().BeTrue();
            dtoDispute.PauseReason.Should().Be("Dispute");

            dtoRevision.IsPaused.Should().BeTrue();
            dtoRevision.PauseReason.Should().Be("RevisionRequest");

            dtoReview.IsPaused.Should().BeTrue();
            dtoReview.PauseReason.Should().Be("SpecialistReview");
        }

        [Fact]
        public async Task GetContractDeliveries_ShouldReturnMappedDtos()
        {
            // Arrange
            using var context = GetContext();
            var delivery = new ContractDelivery
            {
                ContractId = 12,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3),
                DeliveryNote = "Notes here"
            };
            var att = new DeliveryAttachment
            {
                DeliveryId = delivery.Id,
                Type = AttachmentType.File,
                FileName = "spec.pdf"
            };

            context.ContractDeliveries.Add(delivery);
            context.DeliveryAttachments.Add(att);
            await context.SaveChangesAsync();

            var handler = new GetContractDeliveriesQueryHandler(context);
            var query = new GetContractDeliveriesQuery(12);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].DeliveryNote.Should().Be("Notes here");
            result[0].Attachments.Should().HaveCount(1);
            result[0].Attachments[0].FileName.Should().Be("spec.pdf");
        }

        [Fact]
        public async Task GetEscrowSummary_ShouldComputePrecisionMetrics()
        {
            // Arrange
            using var context = GetContext();
            // Funded: 2 transactions (amount 100 each)
            var escrow1 = CreateEscrow(13, 100m);
            var escrow2 = CreateEscrow(13, 100m);
            escrow1.Status = EscrowStatus.Released;

            // Release Transaction logged in audit trail
            var releaseTx = new EscrowTransaction
            {
                ContractId = 13,
                Type = EscrowTransactionType.ReleasedToFreelancer,
                Amount = 100m,
                PlatformFeeFromClient = 5.5m,
                PlatformFeeFromFreelancer = 15m,
                NetToFreelancer = 85m,
                Status = EscrowStatus.Released
            };

            context.EscrowTransactions.Add(escrow1);
            context.EscrowTransactions.Add(escrow2);
            context.EscrowTransactions.Add(releaseTx);
            await context.SaveChangesAsync();

            var handler = new GetEscrowSummaryQueryHandler(context);
            var query = new GetEscrowSummaryQuery(13);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalFunded.Should().Be(200m);
            result.TotalReleased.Should().Be(85m);
            result.TotalRefunded.Should().Be(0m);
            result.PlatformEarned.Should().Be(20.5m); // 5.5 + 15 from escrow1 (which is Released)
            result.CurrentlyHeld.Should().Be(100m); // escrow2 (which is Held)
        }

        [Fact]
        public async Task CreateProposal_ShouldStoreRevisionsAndDuration()
        {
            // Arrange
            using var context = GetContext();
            var job = new JobPost { Id = "job1", Title = "Test Job", ClientId = "client1", Budget = 500m };
            var freelancer = CreateUser("free1", "free@test.com");
            context.JobPosts.Add(job);
            context.Users.Add(freelancer);
            context.Freelancers.Add(new Freelancer { UserId = "free1", Availability = "Full-Time" });
            await context.SaveChangesAsync();

            var handler = new ServiceImplementation.Implementations.Proposals.CreateProposalCommandHandler(context);
            var dto = new ServiceContracts.DTOs.Proposal.ProposalCreateDTO
            {
                JobPostId = "job1",
                BidRate = 450m,
                CoverLetter = "This is a detailed cover letter to pass the validation constraints.",
                MaxRevisions = 5,
                DurationDays = 10
            };
            var command = new ServiceImplementation.Implementations.Proposals.CreateProposalCommand(dto, "free1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.MaxRevisions.Should().Be(5);
            result.Data.DurationDays.Should().Be(10);

            var storedProposal = await context.Proposals.FindAsync(result.Data.Id);
            storedProposal!.MaxRevisions.Should().Be(5);
            storedProposal.DurationDays.Should().Be(10);
        }

        [Fact]
        public async Task AcceptOffer_ShouldUpdateDatesAndSetDueDate()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = new Contract
            {
                Id = 22,
                ClientId = "client1",
                FreelancerId = "free1",
                Status = ContractStatus.Draft,
                MaxRevisions = 4,
                DurationDays = 8,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new AcceptOfferCommandHandler(context);
            var command = new AcceptOfferCommand(22, "free1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var updatedContract = await context.Contracts.FindAsync(22);
            updatedContract!.Status.Should().Be(ContractStatus.Active);
            updatedContract.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            updatedContract.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(8), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task RequestRevision_ShouldEnforceMaxRevisionsCap_AndUnlockWithAdditionalRevision()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = new Contract
            {
                Id = 30,
                ClientId = "client1",
                FreelancerId = "free1",
                Status = ContractStatus.Active,
                MaxRevisions = 1,
                DurationDays = 5
            };
            var delivery = new ContractDelivery
            {
                Id = Guid.NewGuid(),
                ContractId = 30,
                Status = DeliveryStatus.Pending
            };
            // 1 existing revision already used
            var existingRevision = new RevisionRequest
            {
                DeliveryId = delivery.Id,
                RequestedByClientId = "client1",
                Reason = "First revision",
                Status = RevisionStatus.Pending
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.ContractDeliveries.Add(delivery);
            context.RevisionRequests.Add(existingRevision);
            await context.SaveChangesAsync();

            var requestRevisionHandler = new RequestRevisionCommandHandler(context);

            // Act: Request revision again (reaches cap of 1)
            var failCommand = new RequestRevisionCommand(delivery.Id, "client1", "Second revision");
            var failResult = await requestRevisionHandler.Handle(failCommand, CancellationToken.None);

            // Assert: Rejected by cap
            failResult.Succeeded.Should().BeFalse();
            failResult.ErrorCode.Should().Be(ErrorCodes.RevisionLimitExceeded);

            // Act: Client creates request for 2 additional revisions
            var requestAdditionalHandler = new RequestAdditionalRevisionCommandHandler(context);
            var reqAddCommand = new RequestAdditionalRevisionCommand(delivery.Id, "client1", 2, "Need more adjustments");
            var reqAddResult = await requestAdditionalHandler.Handle(reqAddCommand, CancellationToken.None);

            // Assert: Pending additional request created
            reqAddResult.Succeeded.Should().BeTrue();
            reqAddResult.Data.Status.Should().Be(RequestStatus.Pending);

            // Act: Freelancer accepts additional revision request
            var respondHandler = new RespondToAdditionalRevisionCommandHandler(context);
            var respondCommand = new RespondToAdditionalRevisionCommand(reqAddResult.Data.Id, "free1", true);
            var respondResult = await respondHandler.Handle(respondCommand, CancellationToken.None);

            // Assert: Succeeded & MaxRevisions increased from 1 to 3
            respondResult.Succeeded.Should().BeTrue();
            var updatedContract = await context.Contracts.FindAsync(30);
            updatedContract!.MaxRevisions.Should().Be(3);

            // Act: Client attempts revision request again
            var retryCommand = new RequestRevisionCommand(delivery.Id, "client1", "Retry revision");
            var retryResult = await requestRevisionHandler.Handle(retryCommand, CancellationToken.None);

            // Assert: Revision successfully created
            retryResult.Succeeded.Should().BeTrue();
            retryResult.Data.Status.Should().Be(RevisionStatus.Pending);
        }

        [Fact]
        public async Task ApproveDelivery_ShouldAutomaticallyCompleteAndCloseContract()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            var contract = new Contract
            {
                Id = 40,
                ClientId = "client1",
                FreelancerId = "free1",
                Status = ContractStatus.Active
            };
            var delivery = new ContractDelivery
            {
                Id = Guid.NewGuid(),
                ContractId = 40,
                Status = DeliveryStatus.Pending
            };
            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(contract);
            context.ContractDeliveries.Add(delivery);
            context.EscrowTransactions.Add(new EscrowTransaction
            {
                Id = Guid.NewGuid(),
                ContractId = 40,
                ContractMilestoneId = null,
                Amount = 100m,
                Status = EscrowStatus.Held,
                Type = EscrowTransactionType.ClientFunded,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            escrowMock.Setup(e => e.ReleaseToFreelancerAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new ServiceContracts.DTOs.Responses.Result<bool> { Succeeded = true, Data = true });
            var handler = new ApproveDeliveryCommandHandler(context, escrowMock.Object);
            var command = new ApproveDeliveryCommand(delivery.Id, "client1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Data.Status.Should().Be(DeliveryStatus.Approved);
            var updatedContract = await context.Contracts.FindAsync(40);
            updatedContract!.Status.Should().Be(ContractStatus.Completed);
            updatedContract.ClosedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task OfferAutoRevokeService_ShouldCancelOffersOlderThan3Days()
        {
            // Arrange
            using var context = GetContext();
            var client = CreateUser("client1", "client@test.com");
            var freelancer = CreateUser("free1", "free@test.com");
            
            // Stale Offer
            var staleContract = new Contract
            {
                Id = 50,
                ClientId = "client1",
                FreelancerId = "free1",
                Status = ContractStatus.Draft,
                AgreedRate = 200m,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            };

            // Active Offer (not expired)
            var activeContract = new Contract
            {
                Id = 51,
                ClientId = "client1",
                FreelancerId = "free1",
                Status = ContractStatus.Draft,
                AgreedRate = 250m,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(client);
            context.Users.Add(freelancer);
            context.Contracts.Add(staleContract);
            context.Contracts.Add(activeContract);
            await context.SaveChangesAsync();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            
            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            escrowMock.Setup(e => e.RefundToClientAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>()))
                .ReturnsAsync(new ServiceContracts.DTOs.Responses.Result<bool> { Succeeded = true, Data = true });
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(AppDbContext))).Returns(context);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(Services.Wallet.IEscrowService))).Returns(escrowMock.Object);

            var logger = new Mock<ILogger<OfferAutoRevokeService>>().Object;
            var service = new OfferAutoRevokeService(serviceProviderMock.Object, logger);

            // Act
            await service.ProcessExpiredOffersAsync(CancellationToken.None);

            // Assert
            var staleResult = await context.Contracts.FindAsync(50);
            staleResult!.Status.Should().Be(ContractStatus.Closed);
            staleResult.ClosedAt.Should().NotBeNull();

            var activeResult = await context.Contracts.FindAsync(51);
            activeResult!.Status.Should().Be(ContractStatus.Draft); // Untouched
        }

        [Fact]
        public async Task GetDeliverySpecialistReview_ShouldWorkForSpecialistAndIncludeDetails()
        {
            // Arrange
            using var context = GetContext();
            var client = new Entities.Users.User { Id = "client_spec", FullName = "Client User" };
            var freelancer = new Entities.Users.User { Id = "free_spec", FullName = "Freelancer User" };
            var specialist = new Entities.Users.User { Id = "spec_user", FullName = "Specialist User" };
            context.Users.AddRange(client, freelancer, specialist);

            var jobPost = new JobPost { Id = Guid.NewGuid().ToString(), Title = "Build anti-gravity device", ClientId = "client_spec" };
            context.JobPosts.Add(jobPost);

            var contract = new Contract
            {
                Id = 101,
                ClientId = "client_spec",
                FreelancerId = "free_spec",
                JobPostId = jobPost.Id,
                Status = ContractStatus.Active,
                AgreedRate = 1000m,
                CreatedAt = DateTime.UtcNow
            };
            context.Contracts.Add(contract);

            var delivery = new ContractDelivery
            {
                Id = Guid.NewGuid(),
                ContractId = 101,
                Status = DeliveryStatus.Pending,
                DeliveryNote = "Finished the device core",
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };
            context.ContractDeliveries.Add(delivery);

            var attachment = new DeliveryAttachment
            {
                Id = Guid.NewGuid(),
                DeliveryId = delivery.Id,
                Type = AttachmentType.File,
                FileName = "schematics.pdf",
                OriginalFileName = "schematics.pdf",
                FileUrl = "uploads/schematics.pdf"
            };
            context.DeliveryAttachments.Add(attachment);

            var review = new ContractSpecialistReview
            {
                Id = Guid.NewGuid(),
                DeliveryId = delivery.Id,
                RequestedByClientId = "client_spec",
                ReviewerType = ReviewerType.Human,
                RequirementsSummary = "Check anti-gravity equations",
                Status = SpecialistReviewStatus.InProgress,
                AssignedSpecialistId = "spec_user",
                RequestedAt = DateTime.UtcNow
            };
            context.ContractSpecialistReviews.Add(review);
            await context.SaveChangesAsync();

            var handler = new GetDeliverySpecialistReviewQueryHandler(context);
            var query = new GetDeliverySpecialistReviewQuery(delivery.Id, "spec_user");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ContractTitle.Should().Be("Build anti-gravity device");
            result.Data.DeliveryNote.Should().Be("Finished the device core");
            result.Data.Attachments.Should().HaveCount(1);
            result.Data.Attachments[0].FileName.Should().Be("schematics.pdf");
        }

        [Fact]
        public async Task DownloadDeliveryAttachment_ShouldAuthorizeSpecialist()
        {
            // Arrange
            using var context = GetContext();
            var client = new Entities.Users.User { Id = "client_dl", FullName = "Client User" };
            var freelancer = new Entities.Users.User { Id = "free_dl", FullName = "Freelancer User" };
            var specialist = new Entities.Users.User { Id = "spec_dl", FullName = "Specialist User" };
            context.Users.AddRange(client, freelancer, specialist);

            var contract = new Contract
            {
                Id = 102,
                ClientId = "client_dl",
                FreelancerId = "free_dl",
                Status = ContractStatus.Active,
                AgreedRate = 1000m,
                CreatedAt = DateTime.UtcNow
            };
            context.Contracts.Add(contract);

            var delivery = new ContractDelivery
            {
                Id = Guid.NewGuid(),
                ContractId = 102,
                Status = DeliveryStatus.Pending,
                ReviewDeadline = DateTime.UtcNow.AddDays(3)
            };
            context.ContractDeliveries.Add(delivery);

            var attachment = new DeliveryAttachment
            {
                Id = Guid.NewGuid(),
                DeliveryId = delivery.Id,
                Type = AttachmentType.File,
                FileName = "design.pdf",
                OriginalFileName = "design.pdf",
                FileUrl = "uploads/design.pdf"
            };
            context.DeliveryAttachments.Add(attachment);

            var review = new ContractSpecialistReview
            {
                Id = Guid.NewGuid(),
                DeliveryId = delivery.Id,
                RequestedByClientId = "client_dl",
                ReviewerType = ReviewerType.Human,
                RequirementsSummary = "Check Design",
                Status = SpecialistReviewStatus.InProgress,
                AssignedSpecialistId = "spec_dl",
                RequestedAt = DateTime.UtcNow
            };
            context.ContractSpecialistReviews.Add(review);
            await context.SaveChangesAsync();

            var storageMock = new Mock<ServiceContracts.Storage.IFileStorageService>();
            storageMock.Setup(s => s.GetPhysicalPath(It.IsAny<string>())).Returns("C:\\mock\\design.pdf");

            var handler = new DownloadDeliveryAttachmentQueryHandler(context, storageMock.Object);
            var query = new DownloadDeliveryAttachmentQuery(attachment.Id, "spec_dl", false, true);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.OriginalFileName.Should().Be("design.pdf");
        }
    }
}
