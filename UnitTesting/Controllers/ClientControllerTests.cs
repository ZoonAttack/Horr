using FluentAssertions;
using Horr.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts.Client;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.UserDTOs;
using ServiceContracts.DTOs.Proposal;
using Services.Client;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting.Controllers
{
    public class ClientControllerTests
    {
        private readonly Mock<IClientProfileService> _profileServiceMock;
        private readonly Mock<IJobService> _jobServiceMock;
        private readonly ClientController _controller;
        private const string TestUserId = "test-client-id";

        public ClientControllerTests()
        {
            _profileServiceMock = new Mock<IClientProfileService>();
            _jobServiceMock = new Mock<IJobService>();
            _controller = new ClientController(_profileServiceMock.Object, _jobServiceMock.Object);

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMe_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var profileData = new ClientMeDto(TestUserId, "John", "Doe", null, true);
            var result = new Result<ClientMeDto>
            {
                Succeeded = true,
                Data = profileData
            };

            _profileServiceMock.Setup(s => s.GetClientMeAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetMe();

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(profileData);
        }

        [Fact]
        public async Task GetMe_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var result = new Result<ClientMeDto>
            {
                Succeeded = false,
                Message = "Error occurred"
            };

            _profileServiceMock.Setup(s => s.GetClientMeAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetMe();

            // Assert
            var badRequestResult = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { result.ErrorCode, result.Message });
        }

        [Fact]
        public async Task GetOnboarding_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var onboardingData = new ClientOnboardingDto(true, false, true);
            var result = new Result<ClientOnboardingDto>
            {
                Succeeded = true,
                Data = onboardingData
            };

            _profileServiceMock.Setup(s => s.GetClientOnboardingAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetOnboarding();

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(onboardingData);
        }

        [Fact]
        public async Task GetClientJobs_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var jobs = new List<ClientJobSummaryDto>
            {
                new ClientJobSummaryDto { Id = "job1", Title = "Job 1" }
            };
            var result = new Result<List<ClientJobSummaryDto>>
            {
                Succeeded = true,
                Data = jobs
            };

            _jobServiceMock.Setup(s => s.GetClientJobsAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetClientJobs();

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(jobs);
        }

        [Fact]
        public async Task GetClientProposals_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var proposals = new List<ClientProposalSummaryDto>
            {
                new ClientProposalSummaryDto { Id = 1, FreelancerName = "Freelancer 1", JobPostTitle = "Job Title 1" }
            };
            var result = new Result<List<ClientProposalSummaryDto>>
            {
                Succeeded = true,
                Data = proposals
            };

            _jobServiceMock.Setup(s => s.GetClientProposalsAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetClientProposals();

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(proposals);
        }

        [Fact]
        public async Task GetClientProposals_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var result = new Result<List<ClientProposalSummaryDto>>
            {
                Succeeded = false,
                ErrorCode = "ERROR_CODE",
                Message = "An error occurred"
            };

            _jobServiceMock.Setup(s => s.GetClientProposalsAsync(TestUserId))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetClientProposals();

            // Assert
            var badRequestResult = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { result.ErrorCode, result.Message });
        }
    }
}
