using Entities;
using Entities.Enums;
using Entities.Project;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceImplementation.Implementations.Contracts;
using ServiceImplementation.Implementations.Reviews;
using ServiceContracts.DTOs.Review;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Exceptions;
using FluentAssertions;
using UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Services;
using ServiceContracts.Storage;

namespace UnitTesting.Project
{
    public class ContractHandlerTests
    {
        // Alias to match whichever helper method creates the in-memory context
        private AppDbContext GetContext() => DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        // ─── Helper ───────────────────────────────────────────────────────────
        private static Proposal BuildProposal(int id = 1, string freelancerId = "free1")
            => new Proposal
            {
                Id = id,
                JobPostId = "99",
                FreelancerId = freelancerId,
                BidRate = 500,
                CoverLetter = "I'm a great fit",
                Status = ProposalStatus.Submitted   // Submitted = valid for acceptance
            };

        private static Contract BuildDraftContract(int id, Proposal proposal, string clientId = "client1")
            => new Contract
            {
                Id = id,
                ProposalId = proposal.Id,
                Proposal = proposal,
                ClientId = clientId,
                FreelancerId = proposal.FreelancerId,
                AgreedRate = proposal.BidRate,
                Status = ContractStatus.Draft
            };

        // ─── AcceptOffer ──────────────────────────────────────────────────────
        [Fact]
        public async Task AcceptOffer_ShouldActivateContract_AndSetProposalToOffer()
        {
            // Arrange
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "free1", UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var proposal = BuildProposal();
            var contract = BuildDraftContract(1, proposal);
            context.Proposals.Add(proposal);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new AcceptOfferCommandHandler(context);
            var command = new AcceptOfferCommand(contract.Id, proposal.FreelancerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();

            var dbContract = await context.Contracts.FindAsync(contract.Id);
            dbContract.Should().NotBeNull();
            dbContract!.Status.Should().Be(ContractStatus.Active);
            dbContract.AcceptedAt.Should().NotBeNull();

            var dbProposal = await context.Proposals.FindAsync(proposal.Id);
            dbProposal!.Status.Should().Be(ProposalStatus.Active);
        }

        // ─── DeclineOffer ─────────────────────────────────────────────────────
        [Fact]
        public async Task DeclineOffer_ShouldSetContractRejected_AndProposalRejected()
        {
            // Arrange
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "free1", UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var proposal = BuildProposal(id: 2);
            var contract = BuildDraftContract(2, proposal);
            context.Proposals.Add(proposal);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            var handler = new DeclineOfferCommandHandler(context, escrowMock.Object);
            var command = new DeclineOfferCommand(contract.Id, proposal.FreelancerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var dbProposal = await context.Proposals.FindAsync(proposal.Id);
            dbProposal!.Status.Should().Be(ProposalStatus.Rejected);

            var dbContract = await context.Contracts.FindAsync(contract.Id);
            dbContract!.Status.Should().Be(ContractStatus.Rejected);
        }

        // ─── DeliverWork ──────────────────────────────────────────────────────
        [Fact]
        public async Task DeliverWork_ShouldSaveDeliveryWithAttachments()
        {
            // Arrange
            using var context = GetContext();
            var freelancerId = "free1";
            context.Users.Add(new Entities.Users.User { Id = freelancerId, UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var contract = new Contract { Id = 10, ClientId = "client1", FreelancerId = freelancerId, Status = ContractStatus.Active };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            // Mock IFormFile so no disk writes are needed (copy writes to MemoryStream)
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World!";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            var files = new List<IFormFile> { fileMock.Object };

            var storageMock = new Mock<IFileStorageService>();
            storageMock.Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoredFileResult { FileUrl = "/uploads/deliveries/test.txt", OriginalFileName = "test.txt", FileType = ".txt", FileSizeBytes = ms.Length });

            var handler = new DeliverWorkCommandHandler(context, storageMock.Object);
            var command = new DeliverWorkCommand(contract.Id, "Here is my work", freelancerId, files);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.ContractId.Should().Be(contract.Id);

            var delivery = await context.WorkDeliveries
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(d => d.Id == result.Data.Id);
            delivery.Should().NotBeNull();
            delivery!.Attachments.Should().HaveCount(1);
            // Attachment is stored as FileUrl (not FileName)
            delivery.Attachments.First().FileUrl.Should().Contain("test.txt");
        }

        // ─── SubmitReview ─────────────────────────────────────────────────────
        [Fact]
        public async Task SubmitReview_ShouldCompleteContract_WhenBothPartiesReviewed()
        {
            // Arrange
            using var context = GetContext();
            var clientId = "client1";
            var freelancerId = "free1";
            context.Users.Add(new Entities.Users.User { Id = freelancerId, UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var contract = new Contract
            {
                Id = 20,
                ClientId = clientId,
                FreelancerId = freelancerId,
                Status = ContractStatus.Active
            };
            // Add a work delivery so state guard passes
            var delivery = new WorkDelivery { Id = 1, ContractId = 20, Note = "Done", SubmittedAt = DateTime.UtcNow, ActionStatus = ActionStatus.NeedsAttention };
            context.Contracts.Add(contract);
            context.WorkDeliveries.Add(delivery);

            // Add existing review from client
            context.ContractReviews.Add(new Entities.Review.ContractReview
            {
                ContractId = contract.Id,
                ReviewerId = clientId,
                Rating = 5,
                Comment = "Great"
            });
            await context.SaveChangesAsync();

            var handler = new SubmitReviewCommandHandler(context);
            var dto = new ContractReviewCreateDTO { Rating = 4, Comment = "Good client" };
            var command = new SubmitReviewCommand(contract.Id, dto, freelancerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            var dbContract = await context.Contracts.FindAsync(contract.Id);
            dbContract!.Status.Should().Be(ContractStatus.Completed);
        }

        // ─── State Guard: DeliverWork on Closed contract ──────────────────────
        [Fact]
        public async Task DeliverWork_ShouldReturnAccountDeleted_WhenUserDeleted()
        {
            using var context = GetContext();
            var freelancerId = "f1";
            context.Users.Add(new Entities.Users.User { Id = freelancerId, FullName = "Deleted User", IsDeleted = true });
            var contract = new Contract { Id = 30, ClientId = "c1", FreelancerId = freelancerId, Status = ContractStatus.Active };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var storageMock = new Mock<IFileStorageService>();
            var handler = new DeliverWorkCommandHandler(context, storageMock.Object);
            var command = new DeliverWorkCommand(30, "note", freelancerId, new List<IFormFile>());

            var result = await handler.Handle(command, CancellationToken.None);
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.AccountDeleted);
        }

        // ─── GetMyContracts Query ─────────────────────────────────────────────
        [Fact]
        public async Task GetMyContracts_ShouldFilterByFreelancer_AndStatus()
        {
            // Arrange
            using var context = GetContext();
            string freeId = "free_query";
            context.Users.Add(new Entities.Users.User { Id = freeId, UserName = "f1", Email = "f1@t.com", FullName = "Free Query" });
            var c1 = new Contract { Id = 101, FreelancerId = freeId, ClientId = "c1", Status = ContractStatus.Active, CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var c2 = new Contract { Id = 102, FreelancerId = freeId, ClientId = "c1", Status = ContractStatus.Completed, CreatedAt = DateTime.UtcNow };
            var c3 = new Contract { Id = 103, FreelancerId = "other", ClientId = "c1", Status = ContractStatus.Active };
            
            context.Contracts.AddRange(c1, c2, c3);

            // Add needed related data for projection (Jobs, Clients, Freelancers)
            context.Users.Add(new Entities.Users.User { Id = "c1", FullName = "Client One" });
            var jp = new JobPost { Id = "job1", Title = "Fixed Job", ClientId = "c1", Description = "Test" };
            context.JobPosts.Add(jp);
            c1.Proposal = new Proposal { JobPost = jp, FreelancerId = freeId, BidRate=1, CoverLetter="c1" };
            c2.Proposal = new Proposal { JobPost = jp, FreelancerId = freeId, BidRate=1, CoverLetter="c2" };

            await context.SaveChangesAsync();

            var handler = new GetMyContractsQueryHandler(context);
            
            // Act: Query Active contracts for freelancer
            var query = new GetMyContractsQuery(freeId, "Freelancer", ContractStatus.Active, 1, 10);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Items.Should().HaveCount(1);
            result.Data.Items.First().Id.Should().Be(101);
            result.Data.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task CreateDirectOffer_ShouldFail_WhenProposalNotSubmitted()
        {
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "client1", FullName = "Client" });
            context.Users.Add(new Entities.Users.User { Id = "free1", FullName = "Freelancer" });
            context.WalletBalances.Add(new Entities.Payment.WalletBalance { UserId = "client1", BalanceEGP = 1000, LastUpdatedAt = DateTime.UtcNow });
            var jp = new JobPost { Id = "job1", Title = "Job", Description = "Desc", ClientId = "client1", Budget = 1000 };
            context.JobPosts.Add(jp);
            var proposal = new Proposal { Id = 5, JobPostId = "job1", FreelancerId = "free1", Status = ProposalStatus.Rejected, BidRate = 500, CoverLetter = "test" };
            context.Proposals.Add(proposal);
            await context.SaveChangesAsync();

            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            var handler = new CreateDirectOfferCommandHandler(context, escrowMock.Object);
            var command = new CreateDirectOfferCommand
            {
                ClientId = "client1",
                FreelancerId = "free1",
                JobPostId = "job1",
                ProposalId = 5,
                AgreedRate = 500
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.InvalidState);
            result.Message.Should().Contain("submitted");
        }

        [Fact]
        public async Task CreateDirectOffer_ShouldFail_WhenProposalAlreadyHasContract()
        {
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "client1", FullName = "Client" });
            context.Users.Add(new Entities.Users.User { Id = "free1", FullName = "Freelancer" });
            context.WalletBalances.Add(new Entities.Payment.WalletBalance { UserId = "client1", BalanceEGP = 1000, LastUpdatedAt = DateTime.UtcNow });
            var jp = new JobPost { Id = "job1", Title = "Job", Description = "Desc", ClientId = "client1", Budget = 1000 };
            context.JobPosts.Add(jp);
            var proposal = new Proposal { Id = 6, JobPostId = "job1", FreelancerId = "free1", Status = ProposalStatus.Submitted, BidRate = 500, CoverLetter = "test" };
            context.Proposals.Add(proposal);
            var existingContract = new Contract { Id = 99, ClientId = "client1", FreelancerId = "free1", ProposalId = 6, AgreedRate = 500, Status = ContractStatus.Draft };
            context.Contracts.Add(existingContract);
            await context.SaveChangesAsync();

            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            var handler = new CreateDirectOfferCommandHandler(context, escrowMock.Object);
            var command = new CreateDirectOfferCommand
            {
                ClientId = "client1",
                FreelancerId = "free1",
                JobPostId = "job1",
                ProposalId = 6,
                AgreedRate = 500
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.InvalidState);
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task AcceptOffer_ShouldSucceed_WhenProposalIsNull()
        {
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "free1", UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var contract = new Contract
            {
                Id = 50,
                ProposalId = null,
                Proposal = null,
                ClientId = "client1",
                FreelancerId = "free1",
                AgreedRate = 500,
                Status = ContractStatus.Draft
            };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new AcceptOfferCommandHandler(context);
            var command = new AcceptOfferCommand(50, "free1");

            var result = await handler.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();

            var dbContract = await context.Contracts.FindAsync(50);
            dbContract!.Status.Should().Be(ContractStatus.Active);
            dbContract.AcceptedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeclineOffer_ShouldSucceed_WhenProposalIsNull()
        {
            using var context = GetContext();
            context.Users.Add(new Entities.Users.User { Id = "free1", UserName = "free1", Email = "f1@t.com", FullName = "Free 1" });
            var contract = new Contract
            {
                Id = 51,
                ProposalId = null,
                Proposal = null,
                ClientId = "client1",
                FreelancerId = "free1",
                AgreedRate = 500,
                Status = ContractStatus.Draft
            };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var escrowMock = new Mock<Services.Wallet.IEscrowService>();
            var handler = new DeclineOfferCommandHandler(context, escrowMock.Object);
            var command = new DeclineOfferCommand(51, "free1");

            var result = await handler.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();

            var dbContract = await context.Contracts.FindAsync(51);
            dbContract!.Status.Should().Be(ContractStatus.Rejected);
            dbContract.RejectedAt.Should().NotBeNull();
        }
    }
}
