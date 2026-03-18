using System;

namespace ServiceContracts.DTOs.Review
{
    public class ContractReviewCreateDTO
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class ContractReviewReadDTO
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
