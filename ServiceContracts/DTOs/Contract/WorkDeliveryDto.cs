using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class WorkDeliveryDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Note { get; set; } = string.Empty;
        public ActionStatus ActionStatus { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
