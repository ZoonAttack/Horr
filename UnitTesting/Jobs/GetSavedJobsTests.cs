using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using ServiceImplementation.Implementations.JobManagement;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceImplementation.Helpers;

namespace UnitTesting.Jobs
{
    public class GetSavedJobsTests
    {
        [Fact]
        public async Task GetSavedJobsQueryHandler_ShouldReturnSavedJobsInCorrectOrder()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var client = new Entities.Users.User 
            { 
                Id = "client1", 
                FullName = "Client One", 
                Email = "c1@test.com", 
                UserName = "c1@test.com",
                Bio = "Bio",
                Address = "Addr",
                City = "City",
                StateProvince = "State",
                ZipCode = "12345",
                Country = "Egypt"
            };
            var freelancer = new Entities.Users.User 
            { 
                Id = "free1", 
                FullName = "Freelancer One", 
                Email = "free1@test.com", 
                UserName = "free1@test.com",
                Bio = "Bio",
                Address = "Addr",
                City = "City",
                StateProvince = "State",
                ZipCode = "12345",
                Country = "Egypt"
            };
            context.Users.AddRange(client, freelancer);
            context.Categories.Add(new Category { Id = "Test", Name = "Test", Slug = "test" });

            var jobs = new List<JobPost>
            {
                new JobPost { Id = "job1", Title = "Job 1", Description = "Desc 1", CategoryId = "Test", Budget = 100, ClientId = "client1", PostedAt = DateTime.UtcNow.AddDays(-2) },
                new JobPost { Id = "job2", Title = "Job 2", Description = "Desc 2", CategoryId = "Test", Budget = 500, ClientId = "client1", PostedAt = DateTime.UtcNow.AddDays(-1) },
                new JobPost { Id = "job3", Title = "Job 3", Description = "Desc 3", CategoryId = "Test", Budget = 300, ClientId = "client1", PostedAt = DateTime.UtcNow }
            };
            context.JobPosts.AddRange(jobs);

            // Save job1 and job3
            context.SavedJobs.AddRange(new List<SavedJob>
            {
                new SavedJob { FreelancerId = "free1", JobPostId = "job1", SavedAt = DateTime.UtcNow.AddMinutes(-10) },
                new SavedJob { FreelancerId = "free1", JobPostId = "job3", SavedAt = DateTime.UtcNow } // Saved later
            });

            await context.SaveChangesAsync();

            var handler = new GetSavedJobsQueryHandler(context, new Moq.Mock<ServiceContracts.Currency.ICurrencyConverterService>().Object);

            // ACT
            var result = await handler.Handle(new GetSavedJobsQuery("free1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.TotalCount.Should().Be(2);
            result.Data.Items.Should().HaveCount(2);
            result.Data.Items[0].Id.Should().Be("job3"); // order by descending SavedAt
            result.Data.Items[1].Id.Should().Be("job1");
            result.Data.Items[0].IsSaved.Should().BeTrue();
        }

        [Fact]
        public async Task GetSavedJobsQueryHandler_ShouldReturnAccountDeleted_WhenFreelancerIsDeleted()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var freelancer = new Entities.Users.User 
            { 
                Id = "free1", 
                FullName = "Freelancer One", 
                Email = "free1@test.com", 
                UserName = "free1@test.com",
                IsDeleted = true
            };
            context.Users.Add(freelancer);
            await context.SaveChangesAsync();

            var handler = new GetSavedJobsQueryHandler(context, new Moq.Mock<ServiceContracts.Currency.ICurrencyConverterService>().Object);

            // ACT
            var result = await handler.Handle(new GetSavedJobsQuery("free1"), CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.AccountDeleted);
        }
    }
}
