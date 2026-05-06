using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Marketplace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTesting.Project
{
    public class ContractTests
    {
        private async Task<(JobPost, Entities.Project.Proposal)> SetupProposalAsync(AppDbContext context)
        {
            // Add Users first to satisfy FKs
            var client = new Entities.Users.User { Id = "client-1", UserName = "client@test.com", Email = "client@test.com", FullName = "Client User", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "C", Bio = "B" };
            var freelancerUser = new Entities.Users.User { Id = "freelancer-1", UserName = "free@test.com", Email = "free@test.com", FullName = "Freelancer User", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "C", Bio = "B" };
            context.Users.AddRange(client, freelancerUser);
            
            // Seed Category
            context.Categories.Add(new Category { Id = "cat-1", Name = "Development", Slug = "development" });
            
            // Proposal.FreelancerId requires a record in 'freelancers' table
            var freelancerProfile = new Entities.Users.Freelancer { UserId = freelancerUser.Id, Availability = "Full-time", PortfolioUrl = "http://test.com" };
            context.Freelancers.Add(freelancerProfile);
            
            await context.SaveChangesAsync();

            var job = new JobPost
            {
                Title = "Test Job",
                Description = "Test Description",
                ClientId = client.Id,
                Budget = 500,
                PostedAt = DateTime.UtcNow,
                JobType = JobType.FixedPrice,
                CategoryId = "cat-1"
            };
            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            var proposal = new Entities.Project.Proposal
            {
                JobPostId = job.Id,
                FreelancerId = freelancerUser.Id,
                CoverLetter = "I can do this",
                BidRate = 200,
                HORRFee = 20,
                Status = ProposalStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Proposals.Add(proposal);
            await context.SaveChangesAsync();

            return (job, proposal);
        }

        [Fact]
        public async Task Contract_SoftDelete_GlobalQueryFilter_ExcludesDeleted()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateSqliteDbContext(Guid.NewGuid().ToString());
            await context.Database.EnsureCreatedAsync();

            var (_, p1) = await SetupProposalAsync(context);
            
            // Need a second proposal for the second contract because it's 1:1
            var freelancer2User = new Entities.Users.User { Id = "freelancer-2", UserName = "f2@test.com", Email = "f2@test.com", FullName = "Freelancer 2", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "C", Bio = "B" };
            context.Users.Add(freelancer2User);
            
            var freelancer2Profile = new Entities.Users.Freelancer { UserId = freelancer2User.Id, Availability = "Full-time", PortfolioUrl = "http://test.com" };
            context.Freelancers.Add(freelancer2Profile);
            
            await context.SaveChangesAsync();

            var job2 = new JobPost { Title = "J2", Description = "D", ClientId = "client-1", PostedAt = DateTime.UtcNow, JobType = JobType.FixedPrice, CategoryId = "cat-1" };
            context.JobPosts.Add(job2);
            await context.SaveChangesAsync();
            var p2 = new Entities.Project.Proposal { JobPostId = job2.Id, FreelancerId = freelancer2User.Id, CoverLetter = "C", BidRate = 10, HORRFee = 1, Status = ProposalStatus.Active };
            context.Proposals.Add(p2);
            await context.SaveChangesAsync();

            var contract1 = new Contract
            {
                ProposalId = p1.Id,
                ClientId = "client-1",
                FreelancerId = "freelancer-1",
                Status = ContractStatus.Active,
                StartedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var contract2 = new Contract
            {
                ProposalId = p2.Id,
                ClientId = "client-1",
                FreelancerId = freelancer2User.Id,
                Status = ContractStatus.Closed,
                StartedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            context.Contracts.Add(contract1);
            context.Contracts.Add(contract2);
            await context.SaveChangesAsync();

            // ACT
            var activeContracts = await context.Contracts.ToListAsync();
            var allContracts = await context.Contracts.IgnoreQueryFilters().ToListAsync();

            // ASSERT
            activeContracts.Should().HaveCount(1);
            activeContracts.First().ProposalId.Should().Be(p1.Id);

            allContracts.Should().HaveCount(2);
        }

        [Fact]
        public async Task ContractReview_UniqueIndex_ContractIdReviewerId()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateSqliteDbContext(Guid.NewGuid().ToString());
            await context.Database.EnsureCreatedAsync();

            var (_, proposal) = await SetupProposalAsync(context);
            var contract = new Contract { ProposalId = proposal.Id, ClientId = "client-1", FreelancerId = "freelancer-1", Status = ContractStatus.Active, StartedAt = DateTime.UtcNow };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var reviewer = new Entities.Users.User { Id = "user1", UserName = "rev1", Email = "rev1@test.com", FullName = "Reviewer User", Address = "A", City = "C", StateProvince = "S", ZipCode = "Z", Country = "C", Bio = "B" };
            context.Users.Add(reviewer);
            await context.SaveChangesAsync();

            var reviewerId = reviewer.Id;

            var review1 = new Entities.Review.ContractReview
            {
                ContractId = contract.Id,
                ReviewerId = reviewerId,
                Rating = 5,
                Comment = "Great",
                CreatedAt = DateTime.UtcNow
            };

            var review2 = new Entities.Review.ContractReview
            {
                ContractId = contract.Id,
                ReviewerId = reviewerId,
                Rating = 4,
                Comment = "Duplicate",
                CreatedAt = DateTime.UtcNow
            };

            context.ContractReviews.Add(review1);
            await context.SaveChangesAsync();

            // ACT
            context.ContractReviews.Add(review2);
            Func<Task> act = async () => await context.SaveChangesAsync();

            // ASSERT
            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task Contract_ProposalId_IsUnique()
        {
            // ARRANGE
            var dbName = Guid.NewGuid().ToString();
            using var context = DbContextUtility.CreateSqliteDbContext(dbName);
            await context.Database.EnsureCreatedAsync();

            var (_, proposal) = await SetupProposalAsync(context);

            var contract1 = new Contract
            {
                ProposalId = proposal.Id,
                ClientId = "client-1",
                FreelancerId = "freelancer-1",
                Status = ContractStatus.Active,
                StartedAt = DateTime.UtcNow
            };

            context.Contracts.Add(contract1);
            await context.SaveChangesAsync();

            // ACT
            Func<Task> act = async () =>
            {
                using var context2 = DbContextUtility.CreateSqliteDbContext(dbName);
                var contract2 = new Contract
                {
                    ProposalId = proposal.Id,
                    ClientId = "client-1",
                    FreelancerId = "freelancer-1",
                    Status = ContractStatus.Active,
                    StartedAt = DateTime.UtcNow
                };
                context2.Contracts.Add(contract2);
                await context2.SaveChangesAsync();
            };

            // ASSERT
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }
}

