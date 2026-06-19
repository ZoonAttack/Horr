using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Entities.Project;
using Entities.Enums;
using Entities.Users;
using ServiceContracts.DTOs.JobInvitation;
using ServiceContracts.DTOs.Proposal;
using ServiceImplementation.Implementations.ClientImplementation;
using ServiceImplementation.Implementations.Proposals;
using ServiceImplementation.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTesting.Jobs
{
    public class JobInvitationServiceTests
    {
        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static Entities.Users.User MakeUser(string id, string name, UserRole role) =>
            new Entities.Users.User
            {
                Id = id, FullName = name, Role = role,
                UserName = name.Replace(" ", "").ToLower(),
                Bio = "Bio", Address = "Addr", City = "City",
                StateProvince = "S", ZipCode = "1", Country = "C"
            };

        // ─── SendInvitationAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task SendInvitationAsync_ShouldCreateInvitation_WhenInputsAreValid()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var clientId    = "client-1";
            var freelancerId = "freelancer-1";
            var jobId       = "job-1";

            context.Users.AddRange(MakeUser(clientId, "Client User", UserRole.Client),
                                   MakeUser(freelancerId, "Freelancer User", UserRole.Freelancer));
            context.Freelancers.Add(new Freelancer { UserId = freelancerId, Availability = "Full-time" });
            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });
            context.JobPosts.Add(new JobPost { Id = jobId, Title = "Job Title", ClientId = clientId, Description = "Desc", CategoryId = "Cat" });
            await context.SaveChangesAsync();

            var service = new JobInvitationService(context);
            var createDto = new JobInvitationCreateDto
            {
                JobPostId    = jobId,
                FreelancerId = freelancerId,
                Message      = "Please join our project!"
            };

            // ACT
            var result = await service.SendInvitationAsync(clientId, createDto);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.JobPostId.Should().Be(jobId);
            result.Data.FreelancerId.Should().Be(freelancerId);
            result.Data.ClientId.Should().Be(clientId);
            result.Data.Message.Should().Be("Please join our project!");
            result.Data.Status.Should().Be(InvitationStatus.Pending);

            var dbInvitation = await context.JobInvitations.FirstOrDefaultAsync();
            dbInvitation.Should().NotBeNull();
            dbInvitation!.JobPostId.Should().Be(jobId);
            dbInvitation.FreelancerId.Should().Be(freelancerId);
            dbInvitation.Status.Should().Be(InvitationStatus.Pending);
        }

        [Fact]
        public async Task SendInvitationAsync_ShouldReturnFreelancerNotFound_WhenFreelancerDoesNotExist()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var clientId = "client-1";
            var jobId    = "job-1";

            context.Users.Add(MakeUser(clientId, "Client User", UserRole.Client));
            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });
            context.JobPosts.Add(new JobPost { Id = jobId, Title = "Job Title", ClientId = clientId, Description = "Desc", CategoryId = "Cat" });
            await context.SaveChangesAsync();

            var service   = new JobInvitationService(context);
            var createDto = new JobInvitationCreateDto
            {
                JobPostId    = jobId,
                FreelancerId = "non-existent",
                Message      = "Please join!"
            };

            // ACT
            var result = await service.SendInvitationAsync(clientId, createDto);

            // ASSERT
            result.Succeeded.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCodes.FreelancerNotFound);
        }

        // ─── WithdrawInvitationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task WithdrawInvitationAsync_ShouldMarkWithdrawn_WhenPending()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var invitationId = "inv-1";
            var clientId     = "client-1";

            context.JobInvitations.Add(new JobInvitation
            {
                Id           = invitationId,
                JobPostId    = "job-1",
                FreelancerId = "freelancer-1",
                ClientId     = clientId,
                Status       = InvitationStatus.Pending
            });
            await context.SaveChangesAsync();

            var service = new JobInvitationService(context);

            // ACT
            var result = await service.WithdrawInvitationAsync(clientId, invitationId);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            var dbInvitation = await context.JobInvitations.FindAsync(invitationId);
            dbInvitation!.Status.Should().Be(InvitationStatus.Withdrawn);
            dbInvitation.RespondedAt.Should().NotBeNull();
        }

        // ─── DeclineInvitationAsync ──────────────────────────────────────────────

        [Fact]
        public async Task DeclineInvitationAsync_ShouldMarkDeclined_WhenPending()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var invitationId = "inv-1";
            var freelancerId = "freelancer-1";

            context.JobInvitations.Add(new JobInvitation
            {
                Id           = invitationId,
                JobPostId    = "job-1",
                FreelancerId = freelancerId,
                ClientId     = "client-1",
                Status       = InvitationStatus.Pending
            });
            await context.SaveChangesAsync();

            var service = new JobInvitationService(context);

            // ACT
            var result = await service.DeclineInvitationAsync(freelancerId, invitationId);

            // ASSERT
            result.Succeeded.Should().BeTrue();
            var dbInvitation = await context.JobInvitations.FindAsync(invitationId);
            dbInvitation!.Status.Should().Be(InvitationStatus.Declined);
            dbInvitation.RespondedAt.Should().NotBeNull();
        }

        // ─── Auto-acceptance via CreateProposalCommandHandler ────────────────────

        [Fact]
        public async Task CreateProposal_ShouldAutoAcceptPendingInvitation_WhenMatchExists()
        {
            // ARRANGE
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var clientId     = "client-1";
            var freelancerId = "freelancer-1";
            var jobId        = "job-1";
            var invitationId = "inv-1";

            context.Users.AddRange(MakeUser(clientId, "Client User", UserRole.Client),
                                   MakeUser(freelancerId, "Freelancer User", UserRole.Freelancer));
            context.Freelancers.Add(new Freelancer { UserId = freelancerId, Availability = "Full-time" });
            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });
            context.JobPosts.Add(new JobPost { Id = jobId, Title = "Job Title", ClientId = clientId, Description = "Desc", CategoryId = "Cat" });

            // Pending invitation that should be auto-accepted when a proposal is submitted
            context.JobInvitations.Add(new JobInvitation
            {
                Id           = invitationId,
                JobPostId    = jobId,
                FreelancerId = freelancerId,
                ClientId     = clientId,
                Status       = InvitationStatus.Pending
            });
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var command = new CreateProposalCommand(
                new ProposalCreateDTO
                {
                    JobPostId   = jobId,
                    BidRate     = 100,
                    CoverLetter = "This is a cover letter long enough to pass validation in proposal handling."
                },
                freelancerId
            );

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Succeeded.Should().BeTrue();

            var dbInvitation = await context.JobInvitations.FindAsync(invitationId);
            dbInvitation!.Status.Should().Be(InvitationStatus.Accepted);
            dbInvitation.RespondedAt.Should().NotBeNull();
            dbInvitation.ProposalId.Should().Be(result.Data.Id);
        }

        [Fact]
        public async Task CreateProposal_ShouldNotFail_WhenNoPendingInvitationExists()
        {
            // ARRANGE — a freelancer submits a proposal without any invitation (organic apply)
            using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
            var clientId     = "client-1";
            var freelancerId = "freelancer-1";
            var jobId        = "job-1";

            context.Users.AddRange(MakeUser(clientId, "Client User", UserRole.Client),
                                   MakeUser(freelancerId, "Freelancer User", UserRole.Freelancer));
            context.Freelancers.Add(new Freelancer { UserId = freelancerId, Availability = "Full-time" });
            context.Categories.Add(new Category { Id = "Cat", Name = "Category Name", Slug = "cat" });
            context.JobPosts.Add(new JobPost { Id = jobId, Title = "Job Title", ClientId = clientId, Description = "Desc", CategoryId = "Cat" });
            await context.SaveChangesAsync();

            var handler = new CreateProposalCommandHandler(context);
            var command = new CreateProposalCommand(
                new ProposalCreateDTO
                {
                    JobPostId   = jobId,
                    BidRate     = 100,
                    CoverLetter = "This is a cover letter long enough to pass validation in proposal handling."
                },
                freelancerId
            );

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT — proposal created successfully, no invitation side effects
            result.Succeeded.Should().BeTrue();
            var invitationCount = await context.JobInvitations.CountAsync();
            invitationCount.Should().Be(0);
        }
    }
}
