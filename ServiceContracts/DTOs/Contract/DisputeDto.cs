using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class DisputeDto
    {
        public Guid Id { get; set; }
        public int ContractId { get; set; }
        public Guid ContractDeliveryId { get; set; }
        public string OpenedByUserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DisputeStatus Status { get; set; }
        public string? AdminId { get; set; }
        public string? AdminDecision { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
