using Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.JobInvitation
{
    public class JobInvitationCreateDto
    {
        [Required]
        public string JobPostId { get; set; } = string.Empty;

        [Required]
        public string FreelancerId { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Invitation message cannot exceed 1000 characters.")]
        public string? Message { get; set; }
    }

    public class JobInvitationReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string JobPostId { get; set; } = string.Empty;
        public string JobPostTitle { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public string FreelancerName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? Message { get; set; }
        public InvitationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? ProposalId { get; set; }
    }
}
