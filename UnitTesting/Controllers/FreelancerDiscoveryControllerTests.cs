using FluentAssertions;
using Horr.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Moq;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MediatR;
using ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS;
using System.Threading;
using ServiceContracts.DTOs.Skill.FreelancerSkill;

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
            var data = new PagedResult<FreelancerReadDTO>
            {
                Items = new List<FreelancerReadDTO>
                {
                    new FreelancerReadDTO
                    {
                        Id = "freelancer1",
                        FullName = "Jane Doe",
                        Title = "Senior Designer",
                        ProfilePicturePath = "/avatars/jane.jpg",
                        IsVerified = true,
                        TrustScore = 95.0M,
                        AverageRating = 4.8,
                        TotalReviews = 12,
                        IsSaved = true,
                        Skills = new List<FreelancerSkillReadDTO>
                        {
                            new FreelancerSkillReadDTO { SkillName = "Figma", ProficiencyLevel = Entities.Enums.ProficiencyLevel.Expert }
                        }
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };
            var expectedResult = new Result<PagedResult<FreelancerReadDTO>> { Succeeded = true, Data = data };

            _mediatorMock.Setup(m => m.Send<Result<PagedResult<FreelancerReadDTO>>>(It.IsAny<SearchFreelancersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.SearchFreelancers("query", null, null, null, null, null, null, "TrustScore", true, 1, 10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(data);
        }

        [Fact]
        public async Task SaveFreelancer_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send<Result<bool>>(It.IsAny<SaveFreelancerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool> { Succeeded = true, Data = true });

            // Act
            var result = await _controller.SaveFreelancer("freelancer1");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Freelancer saved successfully." });
        }

        [Fact]
        public async Task UnsaveFreelancer_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send<Result<bool>>(It.IsAny<UnsaveFreelancerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool> { Succeeded = true, Data = true });

            // Act
            var result = await _controller.UnsaveFreelancer("freelancer1");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Freelancer removed from saved list." });
        }

        [Fact]
        public async Task GetSavedFreelancers_ShouldReturnOk_WithPagedResult()
        {
            // Arrange
            var data = new PagedResult<FreelancerReadDTO>
            {
                Items = new List<FreelancerReadDTO>
                {
                    new FreelancerReadDTO
                    {
                        Id = "freelancer1",
                        FullName = "Jane Doe",
                        Title = "Senior Designer",
                        ProfilePicturePath = "/avatars/jane.jpg",
                        IsVerified = true,
                        TrustScore = 95.0M,
                        AverageRating = 4.8,
                        TotalReviews = 12,
                        IsSaved = true,
                        Skills = new List<FreelancerSkillReadDTO>
                        {
                            new FreelancerSkillReadDTO { SkillName = "Figma", ProficiencyLevel = Entities.Enums.ProficiencyLevel.Expert }
                        }
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };
            var expectedResult = new Result<PagedResult<FreelancerReadDTO>> { Succeeded = true, Data = data };

            _mediatorMock.Setup(m => m.Send<Result<PagedResult<FreelancerReadDTO>>>(It.IsAny<GetSavedFreelancersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetSavedFreelancers(1, 10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(data);
        }
    }
}
