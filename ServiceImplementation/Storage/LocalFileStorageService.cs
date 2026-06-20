using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ServiceContracts.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _wwwrootPath;
        private readonly HashSet<string> _blockedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".sh", ".ps1", ".msi", ".vbs", ".js", ".jar"
        };

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _wwwrootPath = Path.Combine(env.ContentRootPath, "App_Data");
        }

        public async Task<StoredFileResult> SaveAsync(IFormFile file, string subFolder, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            if (file.Length > 2L * 1024 * 1024 * 1024)
            {
                throw new InvalidOperationException("File size exceeds 2GB limit.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (_blockedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type '{extension}' is not allowed.");
            }

            var folderPath = Path.Combine(_wwwrootPath, "uploads", subFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, storedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            return new StoredFileResult
            {
                FileUrl = $"/uploads/{subFolder}/{storedFileName}",
                OriginalFileName = Path.GetFileName(file.FileName),
                FileType = extension,
                FileSizeBytes = file.Length
            };
        }

        public string? GetPhysicalPath(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return null;
            }

            var path = fileUrl.TrimStart('/');
            path = path.Replace('/', Path.DirectorySeparatorChar);

            var physicalPath = Path.Combine(_wwwrootPath, path);
            return File.Exists(physicalPath) ? physicalPath : null;
        }

        public void Delete(string fileUrl)
        {
            var physicalPath = GetPhysicalPath(fileUrl);
            if (physicalPath != null)
            {
                File.Delete(physicalPath);
            }
        }
    }
}
