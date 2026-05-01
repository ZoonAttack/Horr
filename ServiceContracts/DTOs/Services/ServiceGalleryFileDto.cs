using System;
using Entities.Enums;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceGalleryFileDto
    {
        public string Id { get; set; }
        public string FileUrl { get; set; }
        public ServiceGalleryFileType FileType { get; set; }
        public bool IsCover { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
