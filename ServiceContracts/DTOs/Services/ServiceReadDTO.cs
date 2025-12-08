using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceReadDTO
    {
        public string Id { get; set; }

        public string FreelancerId { get; set; }

        // Optional display information
        public string Freelancer_Name { get; set; }

        public ServiceStatus Status { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public string DeliveryTime { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
