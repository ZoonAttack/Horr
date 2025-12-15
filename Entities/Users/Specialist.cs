using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users
{
    /// <summary>
    /// Profile information for users with the 'specialist' role.
    /// </summary>
    [Table("specialist_profiles")]
    public class Specialist
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        public string ProfilePicturePath { get; set; }

        [MaxLength(100)]
        public string Specialization { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}
