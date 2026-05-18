using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class RevisionRequestDto
    {
        public Guid Id { get; set; }
        public Guid DeliveryId { get; set; }
        public string RequestedByClientId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public RevisionStatus Status { get; set; }
        public string? SpecialistId { get; set; }
        public string? SpecialistDecision { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
