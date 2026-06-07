using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceContracts.Storage
{
    public interface IFileStorageService
    {
        Task<StoredFileResult> SaveAsync(IFormFile file, string subFolder, CancellationToken ct);
        string? GetPhysicalPath(string fileUrl);
        void Delete(string fileUrl);
    }

    public class StoredFileResult
    {
        public string FileUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
    }
}
