using Microsoft.AspNetCore.SignalR;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ServiceImplementation.Hubs
{
    [Authorize]
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
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var conversationIds = await _context.Chats
                    .Where(c => c.ClientId == userId || c.FreelancerId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var conversationId in conversationIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{conversationId}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var conversationIds = await _context.Chats
                    .Where(c => c.ClientId == userId || c.FreelancerId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var conversationId in conversationIds)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{conversationId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatId}");
        }

        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{chatId}");
        }

        public async Task SendMessage(string conversationId, MessageDto messageDto)
        {
            await Clients.Group($"chat-{conversationId}").SendAsync("ReceiveMessage", messageDto);
        }
    }
}
