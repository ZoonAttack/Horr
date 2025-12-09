using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users
{
    /// <summary>
    /// Stores verification documents for a user.
    /// </summary>
    [Table("user_verifications")]
    [Index(nameof(UserId), IsUnique = true)]
    [Index(nameof(Status))]
    public class UserVerification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual Entities.Users.User User { get; set; }

        [MaxLength(255)]
        public string NationalIdImage { get; set; }

        [MaxLength(255)]
        public string SelfieImage { get; set; }

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        public DateTime? ReviewedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}
