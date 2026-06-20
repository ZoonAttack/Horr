using Entities.Communication;
using Entities.Enums;
using Entities.Payment;
using Entities.Review;
using Entities.Token;
using Entities.Verification;
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

    [MaxLength(200)]
    public string? Address { get; set; }
    
    [MaxLength(50)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? StateProvince { get; set; }
    
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    [MaxLength(50)]
    public string? Country { get; set; }
    
    [MaxLength(50)]
    public string? TimeZone { get; set; } = "UTC+02:00";

    [MaxLength(3)]
    public string PreferredCurrency { get; set; } = "USD";

    [Column(TypeName = "text")]
    public string? Bio { get; set; }

    public string? ProfilePicturePath { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100)]
    public decimal TrustScore { get; set; } = 0;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Timestamps (set in application code, not database-generated)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    public virtual ICollection<VerificationRequest> VerificationRequests { get; set; } = new List<VerificationRequest>();
    public virtual Specialist SpecialistProfile { get; set; }
    public virtual Freelancer Freelancer { get; set; }
    public virtual Client Client { get; set; }
    public virtual WalletBalance Wallet { get; set; }
    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; }
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    [InverseProperty("Reviewer")]
    public virtual ICollection<Entities.Review.Review> ReviewsGiven { get; set; } = new List<Entities.Review.Review>();
    [InverseProperty("Reviewee")]
    public virtual ICollection<Entities.Review.Review> ReviewsReceived { get; set; } = new List<Entities.Review.Review>();
    [InverseProperty("Specialist")]
    public virtual ICollection<SpecialistReviewRequest> SpecialistReviewRequests { get; set; } = new List<SpecialistReviewRequest>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
