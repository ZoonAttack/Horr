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
                ErrorCode = "ERROR",
                Message = "Error formatting job"
            };

            _jobServiceMock.Setup(s => s.CreateJobAsync("test-user-id", dto))
                           .ReturnsAsync(resultData);

            // Act
            var result = await _controller.CreateJob(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetJobs_ShouldReturnOk_WithSearchResult()
        {
            // Arrange
            var query = new SearchJobsQuery();
            var searchResult = new SearchJobsQueryResponse
            {
                Items = new List<JobSummaryDto> { new JobSummaryDto { Id = "job1" } },
                TotalCount = 1
            };
            var response = new Result<SearchJobsQueryResponse> { Succeeded = true, Data = searchResult };

            _mediatorMock.Setup(m => m.Send<Result<SearchJobsQueryResponse>>(It.IsAny<SearchJobsQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(response);

            // Act
            var result = await _controller.GetJobs(query);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(searchResult);
        }

        [Fact]
        public async Task GetJob_ShouldReturnOk_WithJobDetails()
        {
            // Arrange
            var jobId = "job123";
            var jobDetails = new JobDetailsDto { Id = jobId };
            var response = new Result<JobDetailsDto> { Succeeded = true, Data = jobDetails };

            _mediatorMock.Setup(m => m.Send<Result<JobDetailsDto>>(It.Is<GetJobDetailsQuery>(q => q.Id == jobId && q.CurrentUserId == "test-user-id"), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(response);

            // Act
            var result = await _controller.GetJob(jobId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(jobDetails);
        }

        [Fact]
        public async Task SaveJob_ShouldReturnOk()
        {
            // Arrange
            var jobId = "job123";
            var response = new Result<bool> { Succeeded = true, Data = true };

            _mediatorMock.Setup(m => m.Send<Result<bool>>(It.IsAny<ToggleSavedJobCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(response);

            // Act
            var result = await _controller.SaveJob(jobId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
