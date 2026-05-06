using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using Entities.Skill;
using Entities.Users;
using ServiceImplementation.Implementations.JobManagement;
using ServiceContracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTesting.Jobs
{
    public class JobManagementTests
    {
        [Fact]
        public async Task SearchJobsQueryHandler_ShouldSortCorrectly()
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
            context.Users.Add(client);
            context.Categories.Add(new Category { Id = "Test", Name = "Test", Slug = "test" });

            var jobs = new List<JobPost>
            {
                new JobPost { Id = "1", Title = "A", Description = "Desc A", CategoryId = "Test", Budget = 100, PostedAt = DateTime.UtcNow.AddDays(-2), ClientId = "client1" },
                new JobPost { Id = "2", Title = "B", Description = "Desc B", CategoryId = "Test", Budget = 500, PostedAt = DateTime.UtcNow.AddDays(-1), ClientId = "client1" },
                new JobPost { Id = "3", Title = "C", Description = "Desc C", CategoryId = "Test", Budget = 300, PostedAt = DateTime.UtcNow, ClientId = "client1" }
            };
            context.JobPosts.AddRange(jobs);
            await context.SaveChangesAsync();

            var handler = new SearchJobsQueryHandler(context);

            // ACT - Newest
            var newest = await handler.Handle(new SearchJobsQuery(SortBy: JobSortEnum.Newest), CancellationToken.None);
            // ASSERT
            newest.Data.Items.First().Id.Should().Be("3");

            // ACT - Oldest
            var oldest = await handler.Handle(new SearchJobsQuery(SortBy: JobSortEnum.Oldest), CancellationToken.None);
            // ASSERT
            oldest.Data.Items.First().Id.Should().Be("1");

            // ACT - Budget
            var budget = await handler.Handle(new SearchJobsQuery(SortBy: JobSortEnum.Budget), CancellationToken.None);
            // ASSERT
            budget.Data.Items.First().Id.Should().Be("2");
        }

        [Fact]
        public async Task ToggleSavedJobCommandHandler_ShouldToggleIdempotently()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var client = new Entities.Users.User 
            { 
                Id = "client1", 
                FullName = "Client One", 
                UserName = "c1",
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
                UserName = "f1",
                Bio = "Bio",
                Address = "Addr",
                City = "City",
                StateProvince = "State",
                ZipCode = "12345",
                Country = "Egypt"
            };
            context.Users.AddRange(client, freelancer);
            context.Categories.Add(new Category { Id = "Test", Name = "Test", Slug = "test" });
            
            var job = new JobPost { Id = "1", Title = "Job 1", Description = "Desc", CategoryId = "Test", ClientId = "client1" };
            context.JobPosts.Add(job);
            await context.SaveChangesAsync();

            var handler = new ToggleSavedJobCommandHandler(context);
            var cmd = new ToggleSavedJobCommand("1", "free1");

            // ACT - Save
            await handler.Handle(cmd, CancellationToken.None);
            // ASSERT
            context.SavedJobs.Count().Should().Be(1);

            // ACT - Unsave (Toggle)
            await handler.Handle(cmd, CancellationToken.None);
            // ASSERT
            context.SavedJobs.Count().Should().Be(0);

            // ACT - Save again (Toggle)
            await handler.Handle(cmd, CancellationToken.None);
            // ASSERT
            context.SavedJobs.Count().Should().Be(1);
        }

        [Fact]
        public async Task SearchJobs_ShouldNotReturnSoftDeletedJobs()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            
            var client = new Entities.Users.User 
            { 
                Id = "client1", 
                FullName = "Client One", 
                UserName = "c1",
                Bio = "Bio",
                Address = "Addr",
                City = "City",
                StateProvince = "State",
                ZipCode = "12345",
                Country = "Egypt"
            };
            context.Users.Add(client);
            context.Categories.Add(new Category { Id = "Test", Name = "Test", Slug = "test" });

            context.JobPosts.Add(new JobPost { Id = "1", Title = "Visible", Description = "Desc", CategoryId = "Test", IsDeleted = false, ClientId = "client1" });
            context.JobPosts.Add(new JobPost { Id = "2", Title = "Hidden", Description = "Desc", CategoryId = "Test", IsDeleted = true, ClientId = "client1" });
            await context.SaveChangesAsync();

            var handler = new SearchJobsQueryHandler(context);

            // ACT
            var result = await handler.Handle(new SearchJobsQuery(), CancellationToken.None);

            // ASSERT
            result.Data.Items.Should().HaveCount(1);
            result.Data.Items.First().Id.Should().Be("1");
        }
    }
}
