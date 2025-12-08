using Entities.Communication;
using Entities.Marketplace;
using Entities.Project;
using Entities.Review;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.User
{
    /// <summary>
    /// Profile information for users with the 'client' role.
    /// </summary>
    [Table("clients")]
    public class Client
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        [Column(TypeName = "text")]
        public string Bio { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }


        // --- Navigation Properties ---
        public virtual User User { get; set; }
        public virtual ICollection<ClientProject> ClientProjects { get; set; } = new List<ClientProject>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public virtual ICollection<SpecialistReviewRequest> SpecialistReviewRequests { get; set; } = new List<SpecialistReviewRequest>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
