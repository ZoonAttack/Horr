using Entities.Enums;
using Entities.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Verification
{
    [Table("verification_requests")]
    public class VerificationRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string FrontImageUrl { get; set; }

        [Required]
        public string BackImageUrl { get; set; }

        [Required]
        public string SelfieUrl { get; set; }

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedByAdminId { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
