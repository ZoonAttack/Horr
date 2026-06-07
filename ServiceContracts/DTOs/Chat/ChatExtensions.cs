using Entities.Communication;
using Entities.Enums;
using System.Linq;

namespace ServiceContracts.DTOs.Chat
{
    public static class ChatExtensions
    {
        /// <summary>
        /// Converts Chat entity to ChatReadDTO
        /// </summary>
        public static ChatReadDTO Chat_To_ChatRead(this Entities.Communication.Chat chat)
        {
            if (chat == null)
            {
                return null;
            }

            return new ChatReadDTO
            {
                Id = chat.Id.ToString(),
                ContractId = chat.ContractId,
                ClientId = chat.ClientId.ToString(),
                FreelancerId = chat.FreelancerId.ToString(),
                CreatedAt = chat.CreatedAt
            };
        }

        /// <summary>
        /// Converts ChatCreateDTO to Chat entity
        /// </summary>
        public static Entities.Communication.Chat ChatCreate_To_Chat(this ChatCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Communication.Chat
            {
                ContractId = createDto.ContractId,
                ClientId = createDto.ClientId,
                FreelancerId = createDto.FreelancerId
            };
        }

        /// <summary>
        /// Converts Message entity to MessageReadDTO
        /// </summary>
        public static MessageReadDTO Message_To_MessageRead(this Message message)
        {
            if (message == null)
            {
                return null;
            }

            return new MessageReadDTO
            {
                Id = message.Id.ToString(),
                ChatId = message.ChatId.ToString(),
                SenderId = message.SenderId,
                Body = message.Body,
                Status = message.Status,
                SentAt = message.SentAt,
                Type = message.Type,
                TextContent = message.TextContent,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                FileSizeBytes = message.FileSizeBytes
            };
        }

        /// <summary>
        /// Converts MessageCreateDTO to Message entity
        /// </summary>
        public static Message MessageCreate_To_Message(this MessageCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Message
            {
                ChatId = createDto.ChatId,
                SenderId = createDto.SenderId,
                Body = createDto.Body,
                Type = createDto.Type,
                TextContent = createDto.TextContent,
                FileUrl = createDto.FileUrl,
                FileName = createDto.FileName,
                FileSizeBytes = createDto.FileSizeBytes
            };
        }

        /// <summary>
        /// Applies MessageUpdateDTO to an existing Message entity
        /// </summary>
        public static void MessageUpdate_To_Message(this Message message, MessageUpdateDTO updateDto)
        {
            if (message == null || updateDto == null)
            {
                return;
            }

            message.Status = updateDto.Status;
        }

        /// <summary>
        /// Converts Chat entity to ChatSummaryDto
        /// </summary>
        public static ChatSummaryDto ToChatSummaryDto(this Entities.Communication.Chat chat, string requestUserId)
        {
            if (chat == null)
            {
                return null;
            }

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
                LastMessageAt = lastMessage?.SentAt,
                UnreadCount = unreadCount
            };
        }

        /// <summary>
        /// Converts Message entity to MessageDto
        /// </summary>
        public static MessageDto ToMessageDto(this Message message)
        {
            if (message == null)
            {
                return null;
            }

            return new MessageDto
            {
                Id = message.Id.ToString(),
                MessageId = message.Id.ToString(),
                ChatId = message.ChatId.ToString(),
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName ?? string.Empty,
                SenderAvatarUrl = message.Sender?.ProfilePicturePath,
                Body = message.Body,
                Status = message.Status,
                SentAt = message.SentAt,
                Type = message.Type,
                TextContent = message.TextContent,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                FileSizeBytes = message.FileSizeBytes
            };
        }
    }
}
