using Entities.Project;
using Entities.Users;
using Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Entities.Communication
{
    /// <summary>
    /// A chat room, typically linked to a specific contract.
    /// </summary>
    [Table("chats")]
    public class Chat : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // --- Navigation Properties ---
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
