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
            var job = new JobPost { Id = "1", Title = "Job", Description = "Desc", ClientId = "c1", CategoryId = "1" };
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
    }
}
