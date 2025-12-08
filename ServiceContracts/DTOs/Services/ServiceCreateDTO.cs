using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceCreateDTO
    {
        public long FreelancerId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public string DeliveryTime { get; set; }

        public ServiceStatus Status { get; set; }
    }
}
