using Entities;
using Entities.Communication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Mappings.Communication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Communication
{
    public class CreateChatCommandHandler : IRequestHandler<CreateChatCommand, Result<ChatSummaryDto>>
    {
        private readonly AppDbContext _context;

        public CreateChatCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ChatSummaryDto>> Handle(CreateChatCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify contract exists
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId && !c.IsDeleted, cancellationToken);

            if (contract == null)
            {
                contract = await _context.Contracts
                    .FirstOrDefaultAsync(c => c.ProposalId == request.ContractId && !c.IsDeleted, cancellationToken);
            }

            if (contract == null)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = $"Contract with ID {request.ContractId} not found."
                };
            }

            // 2. Authorize: Only the client or freelancer of the contract can initiate the chat
            if (contract.ClientId != request.RequestingUserId && contract.FreelancerId != request.RequestingUserId)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Only the contract client or freelancer can initiate a chat for this contract."
                };
            }

            // 3. Check for existing chat linked to this contract
            var existingChat = await _context.Chats
                .Include(c => c.Client.User)
                .Include(c => c.Freelancer.User)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ContractId == request.ContractId && !c.IsDeleted, cancellationToken);

            if (existingChat != null)
            {
                return new Result<ChatSummaryDto>
                {
                    Succeeded = true,
                    Message = "Retrieved existing conversation.",
                    Data = existingChat.ToSummaryDto(request.RequestingUserId)
                };
            }

            // 4. Create new chat session
            var chat = new Chat
            {
                Id = Guid.NewGuid().ToString(),
                ContractId = request.ContractId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch the chat with user navigations populated for proper ToSummaryDto mapping
            var createdChat = await _context.Chats
                .Include(c => c.Client.User)
                .Include(c => c.Freelancer.User)
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == chat.Id, cancellationToken);

            return new Result<ChatSummaryDto>
            {
                Succeeded = true,
                Message = "New conversation initiated successfully.",
                Data = createdChat.ToSummaryDto(request.RequestingUserId)
            };
        }
    }
}
