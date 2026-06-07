namespace ServiceContracts.DTOs.Contract
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int WorkDeliveryId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
