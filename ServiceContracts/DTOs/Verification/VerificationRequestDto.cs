using System;

namespace ServiceContracts.DTOs.Verification
{
    public class VerificationRequestDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string FrontImageUrl { get; set; }
        public string BackImageUrl { get; set; }
        public string SelfieUrl { get; set; }
        public int Status { get; set; }           // 0=Pending, 1=Approved, 2=Rejected
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
