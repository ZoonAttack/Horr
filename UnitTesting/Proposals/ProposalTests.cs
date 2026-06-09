using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using ServiceImplementation.Implementations.Proposals;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Proposal;
using ServiceContracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTesting.Proposals
{
    public class ProposalTests
    {
        [Theory]
        [InlineData(100, 10.00)]
        [InlineData(250.50, 25.05)]
        [InlineData(999.99, 100.00)] // Rounded: 99.999 -> 100.00
        public async Task CreateProposal_ShouldCalculateHORRFeeCorrectly(decimal bidRate, decimal expectedFee)
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "https://example.com" });
            context.Categories.Add(new Category { Id = "1", Name = "1", Slug = "1" });
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1", JobType = JobType.Hourly };
            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = bidRate,
                CoverLetter = new string('a', 60),
                SubmitAsType = SubmitAsType.Freelancer
            };

            // ACT
            var result = await handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.HORRFee.Should().Be(expectedFee);
        }

        [Fact]
        public async Task WithdrawProposal_ShouldSetStatusToWithdrawn()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            var proposal = new Entities.Project.Proposal
            {
                Id = 1,
                JobPostId = "1",
                FreelancerId = "f1",
                Status = ProposalStatus.Active,
                BidRate = 100,
                HORRFee = 10,
                CoverLetter = new string('a', 60)
            };
            context.Proposals.Add(proposal);
            await context.SaveChangesAsync();

            var handler = new WithdrawProposalCommandHandler(context);

            // ACT
            var result = await handler.Handle(new WithdrawProposalCommand(1, "f1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            var updated = await context.Proposals.IgnoreQueryFilters().FirstAsync(p => p.Id == 1);
            updated.Status.Should().Be(ProposalStatus.Withdrawn);
            updated.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task CreateProposal_ShouldReturnAccountDeleted_WhenUserDeleted()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            context.Users.Add(new Entities.Users.User { Id = "f1", FullName = "Deleted User", IsDeleted = true });
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = 100,
                CoverLetter = new string('a', 60)
            };

            // ACT
            var result = await handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.AccountDeleted);
        }

        [Fact]
        public async Task CreateProposal_ShouldFail_WhenMilestonesProvided()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "https://example.com" });
            context.Categories.Add(new Category { Id = "1", Name = "1", Slug = "1" });
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1", JobType = JobType.FixedPrice };
            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = 100,
                CoverLetter = new string('a', 60),
                SubmitAsType = SubmitAsType.Freelancer,
                Terms = new List<ProposalTermDTO>
                {
                    new ProposalTermDTO { MilestoneTitle = "M1", Amount = 100, DueDate = DateTime.UtcNow.AddDays(7) }
                }
            };

            // ACT
            Func<Task> act = async () => await handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Milestone-based proposals are not supported right now"));
        }

        [Fact]
        public async Task CreateProposal_ShouldSucceed_WhenFixedPriceAndNoMilestonesProvided()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "https://example.com" });
            context.Categories.Add(new Category { Id = "1", Name = "1", Slug = "1" });
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1", JobType = JobType.FixedPrice };
            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = 100,
                CoverLetter = new string('a', 60),
                SubmitAsType = SubmitAsType.Freelancer,
                Terms = new List<ProposalTermDTO>() // empty milestones
            };

            // ACT
            var result = await handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.BidRate.Should().Be(100);
            result.Data.Terms.Should().HaveCount(1);
            result.Data.Terms.First().MilestoneTitle.Should().Be("Single Payment");
        }

        [Fact]
        public async Task RejectProposal_ShouldSetStatusToRejected_WhenValidClient()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            context.Categories.Add(new Category { Id = "1", Name = "1", Slug = "1" });
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1", JobType = JobType.FixedPrice };
            context.JobPosts.Add(job);

            var proposal = new Proposal
            {
                Id = 10,
                JobPostId = "1",
                FreelancerId = "f1",
                BidRate = 100,
                Status = ProposalStatus.Submitted,
                CoverLetter = new string('a', 60)
            };
            context.Proposals.Add(proposal);
            await context.SaveChangesAsync();

            var handler = new RejectProposalCommandHandler(context);
            var command = new RejectProposalCommand(10, "c1");

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            var dbProposal = await context.Proposals.FindAsync(10);
            dbProposal!.Status.Should().Be(ProposalStatus.Rejected);
        }

        [Fact]
        public async Task RejectProposal_ShouldReturnUnauthorized_WhenNotJobOwner()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            context.Users.Add(new Entities.Users.User { Id = "f1", UserName = "f1", Email = "f1@t.com", FullName = "F 1" });
            context.Categories.Add(new Category { Id = "1", Name = "1", Slug = "1" });
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1", JobType = JobType.FixedPrice };
            context.JobPosts.Add(job);

            var proposal = new Proposal
            {
                Id = 11,
                JobPostId = "1",
                FreelancerId = "f1",
                BidRate = 100,
                Status = ProposalStatus.Submitted,
                CoverLetter = new string('a', 60)
            };
            context.Proposals.Add(proposal);
            await context.SaveChangesAsync();

            var handler = new RejectProposalCommandHandler(context);
            var command = new RejectProposalCommand(11, "not-the-client");

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ServiceImplementation.Helpers.ErrorCodes.Unauthorized);
        }
    }
}
