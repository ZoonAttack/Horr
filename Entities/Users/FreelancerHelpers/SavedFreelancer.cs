using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    [Table("saved_freelancers")]
    public class SavedFreelancer
    {
        public string ClientId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(ClientId))]
        public virtual Client Client { get; set; } = null!;

        public string FreelancerId { get; set; } = string.Empty;

        [ForeignKey(nameof(FreelancerId))]
        public virtual Freelancer Freelancer { get; set; } = null!;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
