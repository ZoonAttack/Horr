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
using ServiceImplementation.Exceptions;
using FluentAssertions;
using UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Services;

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
            var proposal = BuildProposal();
            var contract = BuildDraftContract(1, proposal);
            context.Proposals.Add(proposal);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new AcceptOfferCommandHandler(context);
            var command = new AcceptOfferCommand(contract.Id, proposal.FreelancerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert — returns bool true
            result.Should().BeTrue();

            var dbContract = await context.Contracts.FindAsync(contract.Id);
            dbContract.Should().NotBeNull();
            dbContract!.Status.Should().Be(ContractStatus.Active);
            dbContract.AcceptedAt.Should().NotBeNull();

            var dbProposal = await context.Proposals.FindAsync(proposal.Id);
            dbProposal!.Status.Should().Be(ProposalStatus.Offer);
        }

        // ─── DeclineOffer ─────────────────────────────────────────────────────
        [Fact]
        public async Task DeclineOffer_ShouldSetContractRejected_AndProposalRejected()
        {
            // Arrange
            using var context = GetContext();
            var proposal = BuildProposal(id: 2);
            var contract = BuildDraftContract(2, proposal);
            context.Proposals.Add(proposal);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new DeclineOfferCommandHandler(context);
            var command = new DeclineOfferCommand(contract.Id, proposal.FreelancerId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
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

            var handler = new DeliverWorkCommandHandler(context);
            var command = new DeliverWorkCommand(contract.Id, "Here is my work", freelancerId, files);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ContractId.Should().Be(contract.Id);

            var delivery = await context.WorkDeliveries
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(d => d.Id == result.Id);
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
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var dbContract = await context.Contracts.FindAsync(contract.Id);
            dbContract!.Status.Should().Be(ContractStatus.Completed);
        }

        // ─── State Guard: DeliverWork on Closed contract ──────────────────────
        [Fact]
        public async Task DeliverWork_ShouldThrow_WhenContractIsClosed()
        {
            using var context = GetContext();
            var contract = new Contract { Id = 30, ClientId = "c1", FreelancerId = "f1", Status = ContractStatus.Closed };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new DeliverWorkCommandHandler(context);
            var command = new DeliverWorkCommand(30, "note", "f1", new List<IFormFile>());

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidStateException>()
                .WithMessage("Cannot deliver work on a closed contract.");
        }

        // ─── State Guard: Duplicate review ────────────────────────────────────
        [Fact]
        public async Task SubmitReview_ShouldThrow_WhenReviewerAlreadyReviewed()
        {
            using var context = GetContext();
            var clientId = "client1";
            var contract = new Contract { Id = 40, ClientId = clientId, FreelancerId = "f1", Status = ContractStatus.Active };
            var delivery = new WorkDelivery { Id = 2, ContractId = 40, Note = "Done", SubmittedAt = DateTime.UtcNow, ActionStatus = ActionStatus.NeedsAttention };
            var existingReview = new Entities.Review.ContractReview { ContractId = 40, ReviewerId = clientId, Rating = 5, Comment = "Great" };
            context.Contracts.Add(contract);
            context.WorkDeliveries.Add(delivery);
            context.ContractReviews.Add(existingReview);
            await context.SaveChangesAsync();

            var handler = new SubmitReviewCommandHandler(context);
            var command = new SubmitReviewCommand(40, new ContractReviewCreateDTO { Rating = 3, Comment = "Again" }, clientId);
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("You have already reviewed this contract.");
        }

        // ─── Validation: SubmitReview Rating ──────────────────────────────────
        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task SubmitReview_ShouldThrowValidationException_WhenRatingIsInvalid(int invalidRating)
        {
            using var context = GetContext();
            var contract = new Contract { Id = 50, ClientId = "c1", FreelancerId = "f1", Status = ContractStatus.Active };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new SubmitReviewCommandHandler(context);
            var command = new SubmitReviewCommand(50, new ContractReviewCreateDTO { Rating = invalidRating, Comment = "Bad" }, "c1");

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Rating"));
        }

        // ─── Validation: DeliverWork Note ─────────────────────────────────────
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeliverWork_ShouldThrowValidationException_WhenNoteIsEmpty(string emptyNote)
        {
            using var context = GetContext();
            var contract = new Contract { Id = 60, ClientId = "c1", FreelancerId = "f1", Status = ContractStatus.Active };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var handler = new DeliverWorkCommandHandler(context);
            var command = new DeliverWorkCommand(60, emptyNote, "f1", null);

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Note"));
        }

        // ─── GetMyContracts Query ─────────────────────────────────────────────
        [Fact]
        public async Task GetMyContracts_ShouldFilterByFreelancer_AndStatus()
        {
            // Arrange
            using var context = GetContext();
            string freeId = "free_query";
            var c1 = new Contract { Id = 101, FreelancerId = freeId, ClientId = "c1", Status = ContractStatus.Active, CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var c2 = new Contract { Id = 102, FreelancerId = freeId, ClientId = "c1", Status = ContractStatus.Completed, CreatedAt = DateTime.UtcNow };
            var c3 = new Contract { Id = 103, FreelancerId = "other", ClientId = "c1", Status = ContractStatus.Active };
            
            context.Contracts.AddRange(c1, c2, c3);

            // Add needed related data for projection (Jobs, Clients, Freelancers)
            context.Users.Add(new Entities.Users.User { Id = "c1", FullName = "Client One" });
            context.Users.Add(new Entities.Users.User { Id = freeId, FullName = "Freelancer One" });
            var jp = new JobPost { Title = "Fixed Job", ClientId = "c1", Description = "Test" };
            context.JobPosts.Add(jp);
            c1.Proposal = new Proposal { JobPost = jp, FreelancerId = freeId, BidRate=1, CoverLetter="c1" };
            c2.Proposal = new Proposal { JobPost = jp, FreelancerId = freeId, BidRate=1, CoverLetter="c2" };

            await context.SaveChangesAsync();

            var handler = new GetMyContractsQueryHandler(context);
            
            // Act: Query Active contracts for freelancer
            var query = new GetMyContractsQuery(freeId, "Freelancer", ContractStatus.Active, 1, 10);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Id.Should().Be(101);
            result.TotalCount.Should().Be(1);
        }
    }
}
