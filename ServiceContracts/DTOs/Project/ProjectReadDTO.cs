using ServiceContracts.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Project
{
    internal class ProjectReadDTO
    {
        public string Owner { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Budget { get; set; }

        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }

        public ContractReadDTO Contract { get; set; }
    }
}
