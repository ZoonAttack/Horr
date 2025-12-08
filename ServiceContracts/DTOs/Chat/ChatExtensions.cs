using Entities.Communication;

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
                ProjectId = chat.ProjectId.ToString(),
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
                ProjectId = long.Parse(createDto.ProjectId),
                ClientId = long.Parse(createDto.ClientId),
                FreelancerId = long.Parse(createDto.FreelancerId)
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
                Content = message.Content,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentType = message.AttachmentType,
                IsRead = message.IsRead,
                SentAt = message.SentAt
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
                ChatId = long.Parse(createDto.ChatId),
                SenderId = createDto.SenderId,
                Content = createDto.Content,
                AttachmentUrl = createDto.AttachmentUrl,
                AttachmentType = createDto.AttachmentType
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

            message.IsRead = updateDto.IsRead;
        }
    }
}
