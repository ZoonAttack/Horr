using FluentAssertions;
using Horr.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Implementations.JobManagement;
using Services.Client;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting.Controllers
{
    public class JobsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IJobService> _jobServiceMock;
        private readonly JobsController _controller;

        public JobsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _jobServiceMock = new Mock<IJobService>();

            _controller = new JobsController(_mediatorMock.Object, _jobServiceMock.Object);

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateJob_ShouldReturnCreatedAtAction_WhenServiceSucceeds()
        {
            // Arrange
            var dto = new JobDetailsDto { Title = "Test Job" };
            var resultData = new Result<JobDetailsDto>
            {
                Succeeded = true,
                Data = new JobDetailsDto { Id = "job123", Title = "Test Job" }
            };

            _jobServiceMock.Setup(s => s.CreateJobAsync("test-user-id", dto))
                           .ReturnsAsync(resultData);

            // Act
            var result = await _controller.CreateJob(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(JobsController.GetJob));
            createdResult.RouteValues["id"].Should().Be("job123");
            createdResult.Value.Should().BeEquivalentTo(resultData.Data);
        }

        [Fact]
        public async Task CreateJob_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var dto = new JobDetailsDto { Title = "Test Job" };
            var resultData = new Result<JobDetailsDto>
            {
                Succeeded = false,
                Errors = new List<string> { "Error formatting job" }
            };

            _jobServiceMock.Setup(s => s.CreateJobAsync("test-user-id", dto))
                           .ReturnsAsync(resultData);

            // Act
            var result = await _controller.CreateJob(dto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(resultData.Errors);
        }

        [Fact]
        public async Task GetJobs_ShouldReturnOk_WithSearchResult()
        {
            // Arrange
            var query = new SearchJobsQuery();
            var response = new SearchJobsQueryResponse
            {
                Items = new List<JobSummaryDto> { new JobSummaryDto { Id = "job1" } },
                TotalCount = 1
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<SearchJobsQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(response);

            // Act
            var result = await _controller.GetJobs(query);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetJob_ShouldReturnOk_WithJobDetails()
        {
            // Arrange
            var jobId = "job123";
            var response = new JobDetailsDto { Id = jobId };

            _mediatorMock.Setup(m => m.Send(It.Is<GetJobDetailsQuery>(q => q.Id == jobId && q.CurrentUserId == "test-user-id"), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(response);

            // Act
            var result = await _controller.GetJob(jobId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task SaveJob_ShouldReturnNoContent()
        {
            // Arrange
            var jobId = "job123";

            _mediatorMock.Setup(m => m.Send(It.Is<ToggleSavedJobCommand>(c => c.JobPostId == jobId && c.FreelancerId == "test-user-id"), It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SaveJob(jobId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}
