using Entities.Communication;
using Entities.Enums;
using FluentAssertions;
using ServiceImplementation.Mappings.Communication;
using Xunit;

namespace UnitTesting.Communication
{
    public class MessagePreviewHelperTests
    {
        // ── Preview boundary tests ───────────────────────────────────────────

        [Fact]
        public void GetPreview_49CharBody_ReturnsAsIs()
        {
            var body = new string('a', 49);
            var result = MessagePreviewHelper.GetPreview(body);
            result.Should().Be(body);
            result.Should().NotEndWith("...");
        }

        [Fact]
        public void GetPreview_50CharBody_ReturnsAsIs()
        {
            var body = new string('b', 50);
            var result = MessagePreviewHelper.GetPreview(body);
            result.Should().Be(body);
            result.Length.Should().Be(50);
            result.Should().NotEndWith("...");
        }

        [Fact]
        public void GetPreview_51CharBody_ReturnsTruncatedWith53TotalLength()
        {
            var body = new string('c', 51);
            var result = MessagePreviewHelper.GetPreview(body);
            result.Should().EndWith("...");
            result.Length.Should().Be(53);
            result.Should().StartWith(new string('c', 50));
        }

        [Fact]
        public void GetPreview_100CharBody_ReturnsTruncatedWith53TotalLength()
        {
            var body = new string('d', 100);
            var result = MessagePreviewHelper.GetPreview(body);
            result.Should().EndWith("...");
            result.Length.Should().Be(53);
            result.Should().StartWith(new string('d', 50));
        }

        [Fact]
        public void GetPreview_EmptyString_ReturnsEmptyString()
        {
            var result = MessagePreviewHelper.GetPreview(string.Empty);
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void GetPreview_Null_ReturnsEmptyString()
        {
            var result = MessagePreviewHelper.GetPreview(null);
            result.Should().Be(string.Empty);
        }
    }

    public class ChatMappingExtensionsTests
    {
        // ── Message.ToDto() ──────────────────────────────────────────────────

        [Fact]
        public void MessageToDto_MapsAllFieldsCorrectly()
        {
            var sentAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var message = new Message
            {
                Id = "msg-abc",
                ChatId = "conv-xyz",
                SenderId = "user-111",
                Body = "Hello, world!",
                Status = MessageStatus.Read,
                SentAt = sentAt
            };

            var dto = message.ToDto();

            dto.Id.Should().Be("msg-abc");
            dto.ChatId.Should().Be("conv-xyz");
            dto.SenderId.Should().Be("user-111");
            dto.Body.Should().Be("Hello, world!");
            dto.Status.Should().Be(MessageStatus.Read);
            dto.SentAt.Should().Be(sentAt);
        }
    }
}
