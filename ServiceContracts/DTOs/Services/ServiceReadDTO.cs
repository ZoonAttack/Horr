using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Services
{
    internal class ServiceReadDTO
    {
        public string Freelancer_Name { get; set; }

        public ServiceStatus Status { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public string Delivery_Time { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
