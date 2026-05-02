using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
