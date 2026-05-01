using FluentAssertions;
using Horr.Controllers.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Settings;
using ServiceContracts.DTOs.Wallet.PaymentMethods;
using ServiceContracts.DTOs.UserDTOs;
using ServiceContracts.Settings;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting.Controllers
{
    public class UserProfileControllerTests
    {
        private readonly Mock<IProfileSettings> _profileSettingsMock;
        private readonly UserProfileController _controller;

        public UserProfileControllerTests()
        {
            _profileSettingsMock = new Mock<IProfileSettings>();
            _controller = new UserProfileController(_profileSettingsMock.Object);

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
        public async Task UpdateName_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var result = new Result<UserProfileDto> { Succeeded = true };
            _profileSettingsMock.Setup(s => s.UpdateFullNameAsync("test-user-id", "New Name"))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateName("New Name");

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task UpdateName_ShouldReturnNotFound_WhenFailed()
        {
            // Arrange
            var result = new Result<UserProfileDto> { Succeeded = false, Errors = new List<string> { "Not found" } };
            _profileSettingsMock.Setup(s => s.UpdateFullNameAsync("test-user-id", "New Name"))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateName("New Name");

            // Assert
            var notFoundResult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(result.Errors);
        }

        [Fact]
        public async Task UpdateEmail_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var result = new Result<UserProfileDto> { Succeeded = true, Message = "Email updated" };
            _profileSettingsMock.Setup(s => s.UpdateEmailAsync("test-user-id", "test@test.com"))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateEmail("test@test.com");

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreatePaymentMethod_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new PaymentMethodCreateDTO();
            var result = new Result<UserProfileDto> { Succeeded = true };
            _profileSettingsMock.Setup(s => s.CreateBillingAsync("test-user-id", dto))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreatePaymentMethod(dto);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateLocation_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new LocationUpdateDto();
            var result = new Result<UserProfileDto> { Succeeded = true };
            _profileSettingsMock.Setup(s => s.UpdateLocationAsync("test-user-id", dto))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateLocation(dto);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPrivacy_ShouldReturnOk_WhenProfileExists()
        {
            // Arrange
            var resultData = new UserProfileDto();
            var result = new Result<UserProfileDto> { Succeeded = true, Data = resultData };
            _profileSettingsMock.Setup(s => s.GetPrivacySettingsAsync("test-user-id"))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetPrivacy();

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task GetPrivacy_ShouldReturnNotFound_WhenProfileMissing()
        {
            // Arrange
            var result = new Result<UserProfileDto> { Succeeded = false, Data = null };
            _profileSettingsMock.Setup(s => s.GetPrivacySettingsAsync("test-user-id"))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetPrivacy();

            // Assert
            var notFoundResult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("Freelancer profile not found for user.");
        }

        [Fact]
        public async Task UpdatePrivacy_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new PrivacyUpdateDto();
            var result = new Result<UserProfileDto> { Succeeded = true };
            _profileSettingsMock.Setup(s => s.UpdatePrivacySettingsAsync("test-user-id", dto))
                                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdatePrivacy(dto);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }
    }
}
