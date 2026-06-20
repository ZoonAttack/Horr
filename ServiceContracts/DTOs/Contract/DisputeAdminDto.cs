using System;
using System.Collections.Generic;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class DisputeAdminDto
    {
        public Guid Id { get; set; }
        public DisputeStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public string OpenedByUserId { get; set; } = string.Empty;
        public string OpenedByUserFullName { get; set; } = string.Empty;
        public string? AdminDecision { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Contract summary
        public int ContractId { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientFullName { get; set; } = string.Empty;
        public string FreelancerId { get; set; } = string.Empty;
        public string FreelancerFullName { get; set; } = string.Empty;
        public decimal AgreedRate { get; set; }
        public ContractStatus ContractStatus { get; set; }

        // Delivery + downloadable attachments
        public Guid DeliveryId { get; set; }
        public List<AttachmentSummaryDto> Attachments { get; set; } = new();
    }

    public class AttachmentSummaryDto
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
    }
}
