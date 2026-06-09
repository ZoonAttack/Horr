using System;
using Entities.Project;

namespace ServiceContracts.DTOs.Contract
{
    public static class DeliveryAttachmentExtensions
    {
        public static AttachmentDto ToDto(this DeliveryAttachment attachment)
        {
            if (attachment == null) return null!;

            return new AttachmentDto
            {
                Id = attachment.Id,
                WorkDeliveryId = attachment.WorkDeliveryId ?? 0,
                DeliveryId = attachment.DeliveryId,
                FileUrl = attachment.FileUrl,
                FileType = attachment.FileType,
                OriginalFileName = attachment.OriginalFileName,
                FileSizeBytes = attachment.FileSizeBytes,
                UploadedAt = attachment.UploadedAt,
                Type = attachment.Type,
                FileName = attachment.FileName,
                StoragePath = attachment.StoragePath,
                Url = attachment.Url
            };
        }

        public static DeliveryAttachment ToEntity(this AttachmentDto dto)
        {
            if (dto == null) return null!;

            return new DeliveryAttachment
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                WorkDeliveryId = dto.WorkDeliveryId,
                DeliveryId = dto.DeliveryId,
                FileUrl = dto.FileUrl ?? string.Empty,
                FileType = dto.FileType ?? string.Empty,
                OriginalFileName = dto.OriginalFileName ?? string.Empty,
                FileSizeBytes = dto.FileSizeBytes,
                UploadedAt = dto.UploadedAt == default ? DateTime.UtcNow : dto.UploadedAt,
                Type = dto.Type,
                FileName = dto.FileName,
                StoragePath = dto.StoragePath,
                Url = dto.Url
            };
        }
    }
}
