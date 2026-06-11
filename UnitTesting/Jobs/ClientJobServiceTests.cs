using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using Entities.Communication;
using ServiceImplementation.Implementations.ClientImplementation;
using ServiceContracts.DTOs.JobManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTesting.Jobs
{
    public class ClientJobServiceTests
    {
        [Fact]
        public async Task GetClientJobsAsync_ShouldReturnCorrectStats()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var clientId = "client1";
            var freelancerId = "free1";

            var client = new Entities.Users.User { Id = clientId, FullName = "Client", Role = UserRole.Client, UserName = "c1", Bio = "Bio", Address = "Addr", City = "City", StateProvince = "S", ZipCode = "1", Country = "C" };
            var freelancer = new Entities.Users.User { Id = freelancerId, FullName = "Free", Role = UserRole.Freelancer, UserName = "f1", Bio = "Bio", Address = "Addr", City = "City", StateProvince = "S", ZipCode = "1", Country = "C" };
            context.Users.AddRange(client, freelancer);

            context.Categories.Add(new Category { Id = "Cat", Name = "Cat", Slug = "cat" });

            var job = new JobPost { Id = "job1", Title = "Job 1", ClientId = clientId, Description = "Desc", CategoryId = "Cat" };
            context.JobPosts.Add(job);

            // Add 2 Proposals
            context.Proposals.Add(new Proposal { Id = 1, JobPostId = "job1", FreelancerId = freelancerId, CoverLetter = "CL1", BidRate = 10, HORRFee = 1 });
            context.Proposals.Add(new Proposal { Id = 2, JobPostId = "job1", FreelancerId = freelancerId, CoverLetter = "CL2", BidRate = 10, HORRFee = 1 });

            // Add 1 Invitation
            context.JobInvitations.Add(new JobInvitation { Id = "inv1", JobPostId = "job1", FreelancerId = freelancerId, ClientId = clientId });

            // Add 1 Conversation linked to job
            context.Conversations.Add(new Conversation { Id = "conv1", JobPostId = "job1" });

            // Add 1 Hired Contract
            context.Contracts.Add(new Contract 
            { 
                Id = 101, 
                JobPostId = "job1", 
                ClientId = clientId, 
                FreelancerId = freelancerId, 
                Status = ContractStatus.Active,
                AgreedRate = 20
            });

            await context.SaveChangesAsync();

            var service = new JobService(context);

            // ACT
            var result = await service.GetClientJobsAsync(clientId);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            var stats = result.Data.First().Stats;
            stats.Proposals.Should().Be(2);
            stats.Invited.Should().Be(1);
            stats.Messaged.Should().Be(1);
            stats.Hired.Should().Be(1);
        }

        [Fact]
        public async Task GetClientProposalsAsync_ShouldReturnProposalsForClientJobs_IncludingJobSummary()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var clientId = "client1";
            var freelancerId = "free1";

            var client = new Entities.Users.User { Id = clientId, FullName = "Client User", Role = UserRole.Client, UserName = "c1", Bio = "Bio", Address = "Addr", City = "City", StateProvince = "S", ZipCode = "1", Country = "C" };
            var freelancerUser = new Entities.Users.User { Id = freelancerId, FullName = "Freelancer User", Role = UserRole.Freelancer, UserName = "f1" };
            var freelancer = new Freelancer { UserId = freelancerId, User = freelancerUser, Availability = "Full-time" };
            context.Users.AddRange(client, freelancerUser);
            context.Freelancers.Add(freelancer);

            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });

            var job = new JobPost { Id = "job1", Title = "Job Title 1", ClientId = clientId, Description = "Desc", CategoryId = "Cat", Budget = 500, JobType = JobType.FixedPrice };
            context.JobPosts.Add(job);

            var proposal = new Proposal 
            { 
                Id = 1, 
                JobPostId = "job1", 
                FreelancerId = freelancerId, 
                Freelancer = freelancer,
                CoverLetter = "Apply Cover Letter", 
                BidRate = 450, 
                HORRFee = 45, 
                Status = ProposalStatus.Submitted, 
                CreatedAt = DateTime.UtcNow 
            };
            context.Proposals.Add(proposal);

            await context.SaveChangesAsync();

            var service = new JobService(context);

            // ACT
            var result = await service.GetClientProposalsAsync(clientId);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            var summary = result.Data.First();
            summary.Id.Should().Be(proposal.Id);
            summary.FreelancerName.Should().Be("Freelancer User");
            summary.BidRate.Should().Be(450);
            summary.CoverLetter.Should().Be("Apply Cover Letter");
            summary.JobPostId.Should().Be("job1");
            summary.JobPostTitle.Should().Be("Job Title 1");
            summary.JobBudget.Should().Be(500);
            summary.JobType.Should().Be(JobType.FixedPrice);
        }

        [Fact]
        public async Task GetClientProposalsAsync_ShouldExcludeDeletedJobsOrProposals()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var clientId = "client1";
            var freelancerId = "free1";

            var client = new Entities.Users.User { Id = clientId, FullName = "Client User", Role = UserRole.Client, UserName = "c1", Bio = "Bio", Address = "Addr", City = "City", StateProvince = "S", ZipCode = "1", Country = "C" };
            var freelancerUser = new Entities.Users.User { Id = freelancerId, FullName = "Freelancer User", Role = UserRole.Freelancer, UserName = "f1" };
            var freelancer = new Freelancer { UserId = freelancerId, User = freelancerUser, Availability = "Full-time" };
            context.Users.AddRange(client, freelancerUser);
            context.Freelancers.Add(freelancer);

            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });

            // Deleted job post
            var jobDeleted = new JobPost { Id = "job-del", Title = "Deleted Job", ClientId = clientId, Description = "Desc", CategoryId = "Cat", IsDeleted = true };
            // Active job post
            var jobActive = new JobPost { Id = "job-act", Title = "Active Job", ClientId = clientId, Description = "Desc", CategoryId = "Cat" };
            context.JobPosts.AddRange(jobDeleted, jobActive);

            // Proposal on deleted job
            context.Proposals.Add(new Proposal { Id = 1, JobPostId = "job-del", FreelancerId = freelancerId, Freelancer = freelancer, CoverLetter = "CL1", BidRate = 10 });
            // Deleted proposal on active job
            context.Proposals.Add(new Proposal { Id = 2, JobPostId = "job-act", FreelancerId = freelancerId, Freelancer = freelancer, CoverLetter = "CL2", BidRate = 10, IsDeleted = true });
            // Active proposal on active job
            context.Proposals.Add(new Proposal { Id = 3, JobPostId = "job-act", FreelancerId = freelancerId, Freelancer = freelancer, CoverLetter = "CL3", BidRate = 10, IsDeleted = false });

            await context.SaveChangesAsync();

            var service = new JobService(context);

            // ACT
            var result = await service.GetClientProposalsAsync(clientId);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Id.Should().Be(3);
        }
    }
}
