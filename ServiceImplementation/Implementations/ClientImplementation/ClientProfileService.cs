using Entities;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.Client;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.UserDTOs;
using ServiceImplementation.Helpers;
using System.Linq;

namespace ServiceImplementation.Implementations.ClientImplementation
{
    public class ClientProfileService : IClientProfileService
    {
        private readonly AppDbContext _db;

        public ClientProfileService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<ClientMeDto>> GetClientMeAsync(string userId)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsDeleted)
            {
                return new Result<ClientMeDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var hasUnreadMessages = await _db.Messages
                .AnyAsync(m => m.Status == MessageStatus.Unread && 
                              m.Conversation.Participants.Any(p => p.UserId == userId) && 
                              m.SenderId != userId);

            var hasPendingProposals = await _db.Proposals
                .AnyAsync(p => p.JobPost.ClientId == userId && p.Status == ProposalStatus.Submitted);

            var dto = new ClientMeDto(
                user.Id,
                user.FullName.Split(" ")[0],
                user.FullName.Split(" ")[1],
                user.ProfilePicturePath,
                hasUnreadMessages || hasPendingProposals
            );

            return new Result<ClientMeDto>
            {
                Succeeded = true,
                Data = dto
            };
        }

        public async Task<Result<ClientOnboardingDto>> GetClientOnboardingAsync(string userId)
        {
            var user = await _db.Users
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsDeleted)
            {
                return new Result<ClientOnboardingDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var dto = new ClientOnboardingDto(
                user.EmailConfirmed,
                user.PaymentMethods.Any(),
                user.PhoneNumberConfirmed
            );

            return new Result<ClientOnboardingDto>
            {
                Succeeded = true,
                Data = dto
            };
        }
    }
}
