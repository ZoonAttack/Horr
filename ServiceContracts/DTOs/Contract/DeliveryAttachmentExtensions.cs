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
                WorkDeliveryId = attachment.WorkDeliveryId,
                FileUrl = attachment.FileUrl,
                FileType = attachment.FileType,
                OriginalFileName = attachment.OriginalFileName,
                FileSizeBytes = attachment.FileSizeBytes,
                UploadedAt = attachment.UploadedAt
            };
        }
    }
}
