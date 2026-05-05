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

            var job = new JobPost { Id = "job1", Title = "Job 1", ClientId = clientId, Description = "Desc", Category = "Cat" };
            context.JobPosts.Add(job);

            // Add 2 Proposals
            context.Proposals.Add(new Proposal { Id = 1, JobPostId = "job1", FreelancerId = freelancerId, CoverLetter = "CL1" });
            context.Proposals.Add(new Proposal { Id = 2, JobPostId = "job1", FreelancerId = freelancerId, CoverLetter = "CL2" });

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
    }
}
