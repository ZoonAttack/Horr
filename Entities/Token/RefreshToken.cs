using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Token
{

    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Token { get; set; } = string.Empty;

        // Lifecycle Management
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;

        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; } = string.Empty;

        // Rotation Tracking: Helps you see which token replaced this one
        public string ReplacedByToken { get; set; } = string.Empty;

        // --- Computed Properties (Not stored in DB) ---
        // These make your if-statements much cleaner

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        [NotMapped]
        public bool IsRevoked => Revoked != null;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        // --- Relationships ---
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
