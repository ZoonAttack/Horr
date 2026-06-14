using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using System.Threading;
using System.Threading.Tasks;
using ServiceImplementation.Mappings.Communication;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetChatByContractQueryHandler : IRequestHandler<GetChatByContractQuery, Result<ChatSummaryDto>>
    {
        private readonly AppDbContext _context;

        public GetChatByContractQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ChatSummaryDto>> Handle(GetChatByContractQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var chat = await _context.Chats
                .Include(c => c.Client.User)
                .Include(c => c.Freelancer.User)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ContractId == request.ContractId, cancellationToken);

            if (chat == null)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = false,
                    ErrorCode = "CONVERSATION_NOT_FOUND",
                    Message = $"Chat for Contract ID {request.ContractId} was not found."
                };
            }

            if (chat.ClientId != request.UserId && chat.FreelancerId != request.UserId)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not a participant in this conversation."
                };
            }

            var result = chat.ToSummaryDto(request.UserId);

            return new Result<ChatSummaryDto> { Succeeded = true, Data = result };
        }
    }
}
