using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractReadDTO
    {
        // Primary identifiers
        public long Id { get; set; }

        public long ProjectId { get; set; }

        public long ClientId { get; set; }

        public long FreelancerId { get; set; }

        // Optional display names
        public string Project_Name { get; set; }

        public string Client_Name { get; set; }

        public string Freelancer_Name { get; set; }

        // Contract details
        public string Terms { get; set; }

        public ContractStatus Status { get; set; }

        public DateTime? SignedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
