using Entities.Communication;
using Entities.Enums;
using Entities.Payment;
using Entities.Review;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.User
{
    /// <summary>
    /// Represents the main user account for all roles.
    /// </summary>
    [Table("users")]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Phone), IsUnique = true)]
    [Index(nameof(Role))]
    [Index(nameof(IsDeleted))]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        public bool IsVerified { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal TrustScore { get; set; } = 0;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual UserVerification UserVerification { get; set; }
        public virtual Specialist SpecialistProfile { get; set; }
        public virtual Freelancer Freelancer { get; set; }
        public virtual Client Client { get; set; }
        public virtual Wallet Wallet { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        [InverseProperty("Reviewer")]
        public virtual ICollection<Entities.Review.Review> ReviewsGiven { get; set; } = new List<Entities.Review.Review>();
        [InverseProperty("Reviewee")]
        public virtual ICollection<Entities.Review.Review> ReviewsReceived { get; set; } = new List<Entities.Review.Review>();
        [InverseProperty("Specialist")]
        public virtual ICollection<SpecialistReviewRequest> SpecialistReviewRequests { get; set; } = new List<SpecialistReviewRequest>();
    }
}
