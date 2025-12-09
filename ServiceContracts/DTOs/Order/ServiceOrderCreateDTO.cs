using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Enums;

namespace ServiceContracts.DTOs.Order
{
    public class ServiceOrderCreateDTO
    {
        public string ClientId { get; set; }

        public string FreelancerId { get; set; }

        public string? ServiceId { get; set; }

        public string? ProjectId { get; set; }

        public OrderType OrderType { get; set; }

        public decimal Amount { get; set; }

    }
}
