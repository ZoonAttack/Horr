using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Reviews
{
    internal class ReviewCreateDTO
    {
        [Required]
        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}
