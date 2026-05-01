using FluentAssertions;
using Horr.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Moq;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MediatR;
using ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS;
using System.Threading;

namespace UnitTesting.Controllers
{
    public class FreelancerDiscoveryControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly FreelancerDiscoveryController _controller;

        public FreelancerDiscoveryControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new FreelancerDiscoveryController(_mediatorMock.Object);

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-client-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task SearchFreelancers_ShouldReturnOk_WithPagedResult()
        {
            // Arrange
            var expectedResult = new PagedResult<FreelancerReadDTO>
            {
                Items = new List<FreelancerReadDTO> { new FreelancerReadDTO { Id = "freelancer1" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFreelancersQuery>(), default))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.SearchFreelancers("query", null, null, null, null, null, null, "TrustScore", true, 1, 10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task SaveFreelancer_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveFreelancerCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SaveFreelancer("freelancer1");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Freelancer saved successfully." });
        }

        [Fact]
        public async Task SaveFreelancer_ShouldReturnBadRequest_WhenFails()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveFreelancerCommand>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SaveFreelancer("freelancer1");

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Failed to save freelancer." });
        }

        [Fact]
        public async Task UnsaveFreelancer_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<UnsaveFreelancerCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UnsaveFreelancer("freelancer1");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Freelancer removed from saved list." });
        }

        [Fact]
        public async Task UnsaveFreelancer_ShouldReturnNotFound_WhenFails()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<UnsaveFreelancerCommand>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UnsaveFreelancer("freelancer1");

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Saved freelancer not found." });
        }

        [Fact]
        public async Task GetSavedFreelancers_ShouldReturnOk_WithPagedResult()
        {
            // Arrange
            var expectedResult = new PagedResult<FreelancerReadDTO>
            {
                Items = new List<FreelancerReadDTO> { new FreelancerReadDTO { Id = "freelancer1" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSavedFreelancersQuery>(), default))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetSavedFreelancers(1, 10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
        }
    }
}
