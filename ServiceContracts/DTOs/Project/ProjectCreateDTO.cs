using Entities.Project;
using ServiceContracts.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Project
{
    internal class ProjectCreateDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Budget { get; set; }

        public ContractCreateDTO Contract{ get; set; }
    }
}
