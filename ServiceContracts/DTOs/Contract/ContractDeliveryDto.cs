using System;
using System.Collections.Generic;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractDeliveryDto
    {
        public Guid Id { get; set; }
        public int ContractId { get; set; }
        public Guid? ContractMilestoneId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? DeliveryNote { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime ReviewDeadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
        public bool IsPaused { get; set; }
        public string? PauseReason { get; set; }
    }
}
