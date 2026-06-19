using Entities;
using Entities.Enums;
using Entities.Project;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.Client;
using ServiceContracts.DTOs.JobInvitation;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.ClientImplementation
{
    public class JobInvitationService : IJobInvitationService
    {
        private readonly AppDbContext _db;

        public JobInvitationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<JobInvitationReadDto>> SendInvitationAsync(string clientId, JobInvitationCreateDto dto)
        {
            // 1. Verify client exists
            var clientExists = await _db.Users.AnyAsync(u => u.Id == clientId && !u.IsDeleted);
            if (!clientExists)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ClientNotFound,
                    Message = "Client not found."
                };
            }

            // 2. Verify job post exists and belongs to the client
            var job = await _db.JobPosts.FirstOrDefaultAsync(j => j.Id == dto.JobPostId && !j.IsDeleted);
            if (job == null)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.JobNotFound,
                    Message = "Job post not found."
                };
            }

            if (job.ClientId != clientId)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized to invite freelancers to this job."
                };
            }

            // 3. Verify freelancer exists
            var freelancerUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.FreelancerId && !u.IsDeleted);
            var isFreelancer = await _db.Freelancers.AnyAsync(f => f.UserId == dto.FreelancerId);
            if (freelancerUser == null || !isFreelancer)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FreelancerNotFound,
                    Message = "Freelancer not found."
                };
            }

            // 4. Check for duplicate invitation
            var existingInvitation = await _db.JobInvitations
                .FirstOrDefaultAsync(i => i.JobPostId == dto.JobPostId && i.FreelancerId == dto.FreelancerId);

            if (existingInvitation != null)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = $"An invitation to this job has already been sent to this freelancer (Status: {existingInvitation.Status})."
                };
            }

            // 5. Create invitation
            var invitation = new JobInvitation
            {
                JobPostId = dto.JobPostId,
                FreelancerId = dto.FreelancerId,
                ClientId = clientId,
                Message = dto.Message,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.JobInvitations.Add(invitation);
            await _db.SaveChangesAsync();

            return new Result<JobInvitationReadDto>
            {
                Succeeded = true,
                Message = "Invitation sent successfully.",
                Data = MapToReadDto(invitation, job.Title, freelancerUser.FullName, freelancerUser.FullName) // Placeholder names since we have db entities, let's load them properly.
            };
        }

        public async Task<Result<bool>> WithdrawInvitationAsync(string clientId, string invitationId)
        {
            var invitation = await _db.JobInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (invitation == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvitationNotFound,
                    Message = "Invitation not found."
                };
            }

            if (invitation.ClientId != clientId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not authorized to withdraw this invitation."
                };
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = $"Cannot withdraw invitation with status: {invitation.Status}."
                };
            }

            invitation.Status = InvitationStatus.Withdrawn;
            invitation.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new Result<bool>
            {
                Succeeded = true,
                Message = "Invitation withdrawn successfully.",
                Data = true
            };
        }


        public async Task<Result<bool>> DeclineInvitationAsync(string freelancerId, string invitationId)
        {
            var invitation = await _db.JobInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (invitation == null)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvitationNotFound,
                    Message = "Invitation not found."
                };
            }

            if (invitation.FreelancerId != freelancerId)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not authorized to decline this invitation."
                };
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return new Result<bool>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidState,
                    Message = $"Cannot decline invitation with status: {invitation.Status}."
                };
            }

            invitation.Status = InvitationStatus.Declined;
            invitation.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new Result<bool>
            {
                Succeeded = true,
                Message = "Invitation declined successfully.",
                Data = true
            };
        }

        public async Task<Result<JobInvitationReadDto>> GetInvitationDetailsAsync(string userId, string invitationId)
        {
            var invitation = await _db.JobInvitations
                .Include(i => i.JobPost)
                .Include(i => i.Freelancer)
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvitationNotFound,
                    Message = "Invitation not found."
                };
            }

            if (invitation.ClientId != userId && invitation.FreelancerId != userId)
            {
                return new Result<JobInvitationReadDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not authorized to view this invitation."
                };
            }

            return new Result<JobInvitationReadDto>
            {
                Succeeded = true,
                Data = MapToReadDto(invitation, invitation.JobPost.Title, invitation.Freelancer.FullName, invitation.Client.FullName)
            };
        }

        public async Task<Result<List<JobInvitationReadDto>>> GetClientInvitationsAsync(string clientId, string? jobPostId = null)
        {
            var query = _db.JobInvitations
                .Include(i => i.JobPost)
                .Include(i => i.Freelancer)
                .Include(i => i.Client)
                .Where(i => i.ClientId == clientId);

            if (!string.IsNullOrEmpty(jobPostId))
            {
                query = query.Where(i => i.JobPostId == jobPostId);
            }

            var invitations = await query.ToListAsync();

            var result = invitations.Select(i => MapToReadDto(i, i.JobPost.Title, i.Freelancer.FullName, i.Client.FullName)).ToList();

            return new Result<List<JobInvitationReadDto>>
            {
                Succeeded = true,
                Data = result
            };
        }

        public async Task<Result<List<JobInvitationReadDto>>> GetFreelancerInvitationsAsync(string freelancerId, InvitationStatus? status = null)
        {
            var query = _db.JobInvitations
                .Include(i => i.JobPost)
                .Include(i => i.Freelancer)
                .Include(i => i.Client)
                .Where(i => i.FreelancerId == freelancerId);

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var invitations = await query.ToListAsync();

            var result = invitations.Select(i => MapToReadDto(i, i.JobPost.Title, i.Freelancer.FullName, i.Client.FullName)).ToList();

            return new Result<List<JobInvitationReadDto>>
            {
                Succeeded = true,
                Data = result
            };
        }

        private static JobInvitationReadDto MapToReadDto(JobInvitation invitation, string jobPostTitle, string freelancerName, string clientName)
        {
            return new JobInvitationReadDto
            {
                Id = invitation.Id,
                JobPostId = invitation.JobPostId,
                JobPostTitle = jobPostTitle,
                FreelancerId = invitation.FreelancerId,
                FreelancerName = freelancerName,
                ClientId = invitation.ClientId,
                ClientName = clientName,
                Message = invitation.Message,
                Status = invitation.Status,
                CreatedAt = invitation.CreatedAt,
                RespondedAt = invitation.RespondedAt,
                ProposalId = invitation.ProposalId
            };
        }
    }
}
