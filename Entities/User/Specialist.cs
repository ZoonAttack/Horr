using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.User
{
    /// <summary>
    /// Profile information for users with the 'specialist' role.
    /// </summary>
    [Table("specialist_profiles")]
    public class Specialist
    {
        [Key]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual Entities.User.User User { get; set; }

        [MaxLength(100)]
        public string Specialization { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ReviewFee { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}
