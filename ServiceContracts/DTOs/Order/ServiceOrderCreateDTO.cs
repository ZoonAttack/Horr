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
        public long ClientId { get; set; }

        public long FreelancerId { get; set; }

        public long? ServiceId { get; set; }

        public long? ProjectId { get; set; }

        public OrderType OrderType { get; set; }

        public decimal Amount { get; set; }

    }
}
