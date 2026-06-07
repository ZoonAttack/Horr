using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetChatsByUserQueryHandler : IRequestHandler<GetChatsByUserQuery, Result<List<ChatSummaryDto>>>
    {
        private readonly AppDbContext _context;

        public GetChatsByUserQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ChatSummaryDto>>> Handle(GetChatsByUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<List<ChatSummaryDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var chats = await _context.Chats
                .Include(c => c.Client.User)
                .Include(c => c.Freelancer.User)
                .Include(c => c.Messages)
                .Where(c => (request.Role == UserRole.Client && c.ClientId == request.UserId) ||
                            (request.Role == UserRole.Freelancer && c.FreelancerId == request.UserId))
                .ToListAsync(cancellationToken);

            if (!chats.Any())
            {
                chats = await _context.Chats
                    .Include(c => c.Client.User)
                    .Include(c => c.Freelancer.User)
                    .Include(c => c.Messages)
                    .Where(c => c.ClientId == request.UserId || c.FreelancerId == request.UserId)
                    .ToListAsync(cancellationToken);
            }

            var result = chats.Select(c => c.ToChatSummaryDto(request.UserId)).ToList();

            return new Result<List<ChatSummaryDto>> { Succeeded = true, Data = result };
        }
    }
}
