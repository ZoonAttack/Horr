using Horr.Hubs;
using Microsoft.AspNetCore.SignalR;
using ServiceContracts.DTOs.Chat;
using UnitTesting.Integration;
using Xunit;
using System.Reflection;
using System.Net;
using FluentAssertions;

namespace UnitTesting.Communication
{
    public class ChatHubScaffoldingTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ChatHubScaffoldingTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ChatHub_IsRegisteredAndReachable()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            // SignalR hubs respond to GET /negotiate or return 405 on raw GET.
            // A 404 would mean it's not registered.
            var response = await client.GetAsync("/chatHub");

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public void SendMessage_HasExactSignature()
        {
            // Arrange
            var method = typeof(ChatHub).GetMethod("SendMessage");

            // Assert
            method.Should().NotBeNull();
            var parameters = method!.GetParameters();
            
            parameters.Length.Should().Be(2);
            
            parameters[0].Name.Should().Be("conversationId");
            parameters[0].ParameterType.Should().Be(typeof(string));

            parameters[1].Name.Should().Be("messageDto");
            parameters[1].ParameterType.Should().Be(typeof(MessageDto));

            method.ReturnType.Should().Be(typeof(Task));
        }

        [Fact]
        public void OnConnectedAsync_ContainsVerbatimTodoComment()
        {
            // Arrange
            var hubFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Horr", "Hubs", "ChatHub.cs");
            
            // Fallback for different test execution paths
            if (!File.Exists(hubFilePath))
            {
                // Try absolute path if relative fails (assuming H: drive)
                hubFilePath = @"H:\.NET\Grad\Horr\Horr\Hubs\ChatHub.cs";
            }

            // Act
            var content = File.ReadAllText(hubFilePath);

            // Assert
            content.Should().Contain("// TODO: Implement hub authentication before production deployment");
        }
    }
}
