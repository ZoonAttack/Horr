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
        public int? ProposalId { get; set; }

        public string ClientId { get; set; }

        public string FreelancerId { get; set; }

        // Optional display names
        public string Proposal_Title { get; set; }

        public string Client_Name { get; set; }

        public string Freelancer_Name { get; set; } = string.Empty;

        public decimal AgreedRate { get; set; }

    }
}
