using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using ServiceImplementation.Implementations.Proposals;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Proposal;
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
            
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", Category = "Test", ClientId = "c1" };
            context.JobPosts.Add(job);
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "http://test.com" });
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
            result.HORRFee.Should().Be(expectedFee);
        }

        [Fact]
        public async Task CreateProposal_MissingCoverLetter_ShouldThrowValidationException()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "http://test.com" });
            await context.SaveChangesAsync();
            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = 100,
                CoverLetter = "", // Missing
                SubmitAsType = SubmitAsType.Freelancer
            };

            // ACT
            var act = () => handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("CoverLetter"));
        }

        [Fact]
        public async Task WithdrawProposal_ShouldSetStatusToWithdrawn()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
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
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "http://test.com" });
            await context.SaveChangesAsync();

            var handler = new WithdrawProposalCommandHandler(context);

            // ACT
            await handler.Handle(new WithdrawProposalCommand(1, "f1"), CancellationToken.None);

            // ASSERT
            var updated = await context.Proposals.IgnoreQueryFilters().FirstAsync(p => p.Id == 1);
            updated.Status.Should().Be(ProposalStatus.Withdrawn);
            updated.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task CreateProposal_Duplicate_ShouldThrowConflictException()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            context.Freelancers.Add(new Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "https://example.com" });
            context.Proposals.Add(new Entities.Project.Proposal
            {
                Id = 1,
                JobPostId = "1",
                FreelancerId = "f1",
                CoverLetter = new string('a', 60)
            });
            context.Freelancers.Add(new Entities.Users.Freelancer { UserId = "f1", Availability = "Full-time", PortfolioUrl = "http://test.com" });
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var dto = new ProposalCreateDTO
            {
                JobPostId = "1",
                BidRate = 100,
                CoverLetter = new string('a', 60)
            };

            // ACT
            var act = () => handler.Handle(new CreateProposalCommand(dto, "f1"), CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<ConflictException>();
        }
    }
}
