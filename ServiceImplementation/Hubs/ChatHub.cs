using Microsoft.AspNetCore.SignalR;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;

namespace ServiceImplementation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            // TODO: Implement hub authentication before production deployment
            
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                var conversationIds = await _context.ConversationParticipants
                    .Where(p => p.UserId == userId)
                    .Select(p => p.ConversationId)
                    .ToListAsync();

                foreach (var conversationId in conversationIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                var conversationIds = await _context.ConversationParticipants
                    .Where(p => p.UserId == userId)
                    .Select(p => p.ConversationId)
                    .ToListAsync();

                foreach (var conversationId in conversationIds)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string conversationId, MessageDto messageDto)
        {
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", messageDto);
        }
    }
}
