using Entities.Enums;
using Entities.Project;
using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Contract
{
    // Request body — client sends when requesting review
    public class RequestSpecialistReviewDto
    {
        [Required]
        public ReviewerType ReviewerType { get; set; }

        [Required]
        [MinLength(50)]
        [MaxLength(2000)]
        public string RequirementsSummary { get; set; } = string.Empty;
    }

    // Response — returned to client after review is created or completed
    public class ContractSpecialistReviewReadDto
    {
        public Guid Id { get; set; }
        public Guid DeliveryId { get; set; }
        public ReviewerType ReviewerType { get; set; }
        public SpecialistReviewStatus Status { get; set; }
        public string? AssignedSpecialistId { get; set; }
        public string RequirementsSummary { get; set; } = string.Empty;
        public ReviewVerdict? Verdict { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ContractTitle { get; set; }
        public int? ContractId { get; set; }
        public string? DeliveryNote { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    // Request body — human specialist submits their verdict
    public class SubmitSpecialistReviewDto
    {
        [Required]
        public ReviewVerdict Verdict { get; set; }

        [Required]
        [MinLength(50)]
        [MaxLength(5000)]
        public string ReviewNote { get; set; } = string.Empty;
    }

    public static class ContractSpecialistReviewExtensions
    {
        public static ContractSpecialistReviewReadDto ToReadDto(this ContractSpecialistReview review) => new()
        {
            Id = review.Id,
            DeliveryId = review.DeliveryId,
            ReviewerType = review.ReviewerType,
            Status = review.Status,
            AssignedSpecialistId = review.AssignedSpecialistId,
            RequirementsSummary = review.RequirementsSummary,
            Verdict = review.Verdict,
            ReviewNote = review.ReviewNote,
            RequestedAt = review.RequestedAt,
            CompletedAt = review.CompletedAt,
            ContractTitle = review.Delivery?.Contract?.Proposal?.JobPost?.Title ?? review.Delivery?.Contract?.JobPost?.Title ?? "Direct Offer",
            ContractId = review.Delivery?.ContractId,
            DeliveryNote = review.Delivery?.DeliveryNote,
            Attachments = review.Delivery?.Attachments?.Select(a => a.ToDto()).ToList() ?? new List<AttachmentDto>()
        };
    }
}
