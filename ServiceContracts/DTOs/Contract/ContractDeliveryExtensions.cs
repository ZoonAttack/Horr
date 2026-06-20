using System;
using System.Linq;
using System.Collections.Generic;
using Entities.Project;

using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public static class ContractDeliveryExtensions
    {
        public static ContractDeliveryDto ToDto(this ContractDelivery delivery)
        {
            if (delivery == null) return null!;

            bool hasActiveDispute = delivery.Disputes?.Any(d => !d.IsDeleted && (d.Status == DisputeStatus.Open || d.Status == DisputeStatus.UnderReview)) ?? false;
            bool hasActiveRevision = delivery.RevisionRequests?.Any(r => !r.IsDeleted && (r.Status == RevisionStatus.Pending || r.Status == RevisionStatus.AcceptedBySpecialist)) ?? false;
            bool hasActiveReview = delivery.SpecialistReviews?.Any(r => !r.IsDeleted && (r.Status == SpecialistReviewStatus.Pending || r.Status == SpecialistReviewStatus.InProgress)) ?? false;

            string? pauseReason = null;
            if (hasActiveDispute) pauseReason = "Dispute";
            else if (hasActiveRevision) pauseReason = "RevisionRequest";
            else if (hasActiveReview) pauseReason = "SpecialistReview";

            return new ContractDeliveryDto
            {
                Id = delivery.Id,
                ContractId = delivery.ContractId,
                ContractMilestoneId = delivery.ContractMilestoneId,
                SubmittedAt = delivery.SubmittedAt,
                DeliveryNote = delivery.DeliveryNote,
                Status = delivery.Status,
                ReviewDeadline = delivery.ReviewDeadline,
                CompletedAt = delivery.CompletedAt,
                Attachments = delivery.Attachments?.Select(a => a.ToDto()).ToList() ?? new List<AttachmentDto>(),
                IsPaused = hasActiveDispute || hasActiveRevision || hasActiveReview,
                PauseReason = pauseReason
            };
        }

        public static RevisionRequestDto ToDto(this RevisionRequest request)
        {
            if (request == null) return null!;

            return new RevisionRequestDto
            {
                Id = request.Id,
                DeliveryId = request.DeliveryId,
                RequestedByClientId = request.RequestedByClientId,
                Reason = request.Reason,
                RequestedAt = request.RequestedAt,
                Status = request.Status,
                SpecialistId = request.SpecialistId,
                SpecialistDecision = request.SpecialistDecision,
                ResolvedAt = request.ResolvedAt
            };
        }

        public static DisputeDto ToDto(this Dispute dispute)
        {
            if (dispute == null) return null!;

            return new DisputeDto
            {
                Id = dispute.Id,
                ContractId = dispute.ContractId,
                ContractDeliveryId = dispute.ContractDeliveryId,
                OpenedByUserId = dispute.OpenedByUserId,
                Reason = dispute.Reason,
                OpenedAt = dispute.OpenedAt,
                Status = dispute.Status,
                AdminId = dispute.AdminId,
                AdminDecision = dispute.AdminDecision,
                ResolvedAt = dispute.ResolvedAt
            };
        }
    }
}
