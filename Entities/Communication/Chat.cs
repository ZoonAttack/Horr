using Entities.Project;
using Entities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Entities.Communication
{
    /// <summary>
    /// A chat room, typically linked to a specific project.
    /// </summary>
    [Table("chats")]
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public string ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
