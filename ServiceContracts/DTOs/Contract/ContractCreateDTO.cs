using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Contract
{
    public class ContractCreateDTO
    {
        // Identifiers used for persistence and mapping
        public long ProjectId { get; set; }

        public long ClientId { get; set; }

        public long FreelancerId { get; set; }

        // Optional display names
        public string Project_Name { get; set; }

        public string Client_Name { get; set; }

        public string Freelancer_Name { get; set; } = string.Empty;

        public string Terms { get; set; }

    }
}
