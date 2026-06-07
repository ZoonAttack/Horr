using System;

namespace ServiceContracts.DTOs.Chat
{
    public class ChatSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public int ContractId { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public string? OtherPartyAvatarUrl { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
