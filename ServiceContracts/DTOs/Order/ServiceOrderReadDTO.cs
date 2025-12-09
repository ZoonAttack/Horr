using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Enums;

namespace ServiceContracts.DTOs.Order
{
    public class OrderReadDTO
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public string FreelancerId { get; set; }

        public long? ServiceId { get; set; }

        public long? ProjectId { get; set; }

        public OrderType OrderType { get; set; }

        public decimal Amount { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
