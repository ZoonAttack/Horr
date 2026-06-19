using Entities.Enums;
using ServiceContracts.DTOs.JobInvitation;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceContracts.Client
{
    public interface IJobInvitationService
    {
        // For Client: Send invitation to freelancer
        Task<Result<JobInvitationReadDto>> SendInvitationAsync(string clientId, JobInvitationCreateDto dto);

        // For Client: Withdraw invitation
        Task<Result<bool>> WithdrawInvitationAsync(string clientId, string invitationId);

        // For Freelancer: Decline invitation
        Task<Result<bool>> DeclineInvitationAsync(string freelancerId, string invitationId);

        // Get invitation details
        Task<Result<JobInvitationReadDto>> GetInvitationDetailsAsync(string userId, string invitationId);

        // For Client: List invitations they sent (with optional job post filtering)
        Task<Result<List<JobInvitationReadDto>>> GetClientInvitationsAsync(string clientId, string? jobPostId = null);

        // For Freelancer: List invitations they received
        Task<Result<List<JobInvitationReadDto>>> GetFreelancerInvitationsAsync(string freelancerId, InvitationStatus? status = null);
    }
}

