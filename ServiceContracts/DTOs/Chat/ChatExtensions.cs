using Entities.Communication;
using Entities.Enums;

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
    }
}
