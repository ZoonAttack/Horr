using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Contract
{
    internal class ContractReadDTO
    {
        public string Project_Name { get; set; }
        public string Client_Name { get; set; }

        public string Freelancer_Name { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
