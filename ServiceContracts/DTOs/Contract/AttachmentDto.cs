using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public int? WorkDeliveryId { get; set; }
        public Guid? DeliveryId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }

        // New properties for Phase 3 EARS
        public AttachmentType Type { get; set; } = AttachmentType.File;
        public string? FileName { get; set; }
        public string? StoragePath { get; set; }
        public string? Url { get; set; }
    }
}
