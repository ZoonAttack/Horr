namespace ServiceContracts.DTOs.Contract
{
    public class DownloadFileResult
    {
        public string PhysicalPath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
