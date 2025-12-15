using Entities.Communication;
using Entities.Enums;
using Entities.Payment;
using Entities.Review;
using Entities.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users;

/// <summary>
/// Represents the main user account for all roles.
/// </summary>
[Table("users")]
[Index(nameof(IsDeleted))]
public class User : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public bool IsVerified { get; set; } = false;

    [Column(TypeName = "text")]
    public string Bio { get; set; }

    public string ProfilePicturePath { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Timestamps (set in application code, not database-generated)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
