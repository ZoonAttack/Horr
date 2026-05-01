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
                ProjectId = createDto.ProjectId,
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
                ConversationId = message.ConversationId.ToString(),
                SenderId = message.SenderId,
                Body = message.Body,
                Status = message.Status,
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
                ConversationId = createDto.ConversationId,
                SenderId = createDto.SenderId,
                Body = createDto.Body
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
