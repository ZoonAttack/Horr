using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class AdditionalRevisionRequestDto
    {
        public Guid Id { get; set; }
        public int ContractId { get; set; }
        public Guid DeliveryId { get; set; }
        public int RequestedCount { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}