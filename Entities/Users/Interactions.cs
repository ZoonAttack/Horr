using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Users
{
    [Table("user_interactions")]
    [Index(nameof(UserId), nameof(TargetType), nameof(CreatedAt))]
    public class Interactions
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string TargetId { get; set; } = string.Empty;
        [Required] // "job" or "freelancer"
        public string TargetType { get; set; } = string.Empty;
        [Required] // "view", "save", "apply", "hire"
        public InteractionTypes Action { get; set; } = InteractionTypes.View;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
