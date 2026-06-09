using Entities.Communication;
using Entities.Enums;
using ServiceContracts.DTOs.Chat;
using System.Linq;

namespace ServiceImplementation.Mappings.Communication
{
    public static class ChatMappingExtensions
    {
        public static ChatSummaryDto ToSummaryDto(this Chat chat, string requestUserId)
        {
            if (chat == null) return null!;

            var otherPartyUser = (chat.ClientId == requestUserId)
                ? chat.Freelancer?.User
                : chat.Client?.User;

            var lastMessage = chat.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault();
            string preview = string.Empty;
            if (lastMessage != null)
            {
                preview = lastMessage.Type switch
                {
                    MessageType.Text => string.IsNullOrEmpty(lastMessage.Body)
                        ? string.Empty
                        : (lastMessage.Body.Length <= 60 ? lastMessage.Body : lastMessage.Body.Substring(0, 60)),
                    MessageType.Image => "[Image]",
                    MessageType.Video => "[Video]",
                    MessageType.Pdf => "[PDF]",
                    _ => "[File]"
                };
            }

            var unreadCount = chat.Messages?
                .Count(m => m.SenderId != requestUserId && m.Status == MessageStatus.Unread) ?? 0;

            return new ChatSummaryDto
            {
                Id = chat.Id.ToString(),
                ChatId = chat.Id.ToString(),
                ContractId = chat.ContractId,
                OtherPartyName = otherPartyUser?.FullName ?? string.Empty,
                OtherPartyAvatarUrl = otherPartyUser?.ProfilePicturePath,
                LastMessagePreview = preview,
                LastMessageAt = lastMessage?.SentAt ?? default,
                UnreadCount = unreadCount
            };
        }
    }
}
