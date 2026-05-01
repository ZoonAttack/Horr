using FluentAssertions;
using Horr.Controllers.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ApplicationUser = Entities.Users.User;
using ServiceContracts.DTOs.Responses;
using Services.Authentication;
using Services.DTOs.Authentication;
using Services.DTOs.UserDTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace UnitTesting.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            
            // Passing null for SignInManager since we don't test Logout/ChangeEmail in these unit tests
            _controller = new AuthController(_authServiceMock.Object, null!);

            // Mock User Identity & HttpContext (for cookies)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));

            var httpContext = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new ChangePasswordRequestDTO { OldPassword = "Old", NewPassword = "New" };
            var result = new Result<AuthResponse> { Succeeded = true };

            _authServiceMock.Setup(s => s.ChangePasswordAsync("test-user-id", dto))
                            .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.ChangePassword(dto);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new RegisterRequestDto();
            var result = new Result<AuthResponse> { Succeeded = true };

            _authServiceMock.Setup(s => s.RegisterAsync(dto))
                            .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Register(dto);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithToken_WhenSuccessful()
        {
            // Arrange
            var dto = new LoginRequestDTO();
            var result = new Result<AuthResponse> 
            { 
                Succeeded = true, 
                Data = new AuthResponse { Token = "jwt-token", RefreshToken = "refresh-token" } 
            };

            _authServiceMock.Setup(s => s.LoginAsync(dto))
                            .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.Login(dto);

            // Assert
            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("jwt-token");
            
            // Check if cookie was set
            var cookies = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
            cookies.Should().Contain("refreshToken=refresh-token");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsMissing()
        {
            // Arrange
            // Cookie is not set 

            // Act
            var actionResult = await _controller.RefreshToken();

            // Assert
            var unauthResult = actionResult.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthResult.Value.Should().BeEquivalentTo(new { Message = "Refresh token is missing." });
        }
    }
}
