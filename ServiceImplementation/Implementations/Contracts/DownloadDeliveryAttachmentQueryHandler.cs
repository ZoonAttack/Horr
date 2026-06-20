using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.Storage;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DownloadDeliveryAttachmentQueryHandler : IRequestHandler<DownloadDeliveryAttachmentQuery, Result<DownloadFileResult>>
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DownloadDeliveryAttachmentQueryHandler(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<DownloadFileResult>> Handle(DownloadDeliveryAttachmentQuery request, CancellationToken cancellationToken)
        {
            var attachment = await _context.DeliveryAttachments
                .Include(a => a.WorkDelivery)
                    .ThenInclude(wd => wd!.Contract)
                .Include(a => a.Delivery)
                    .ThenInclude(cd => cd!.Contract)
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

            if (attachment == null)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AttachmentNotFound,
                    Message = "Attachment not found."
                };
            }

            var contract = attachment.Delivery?.Contract ?? attachment.WorkDelivery?.Contract;
            if (contract == null)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FileNotFound,
                    Message = "Contract for this attachment could not be found."
                };
            }

            bool isAuthorized = contract.ClientId == request.RequestingUserId || contract.FreelancerId == request.RequestingUserId;

            if (!isAuthorized && request.IsAdmin)
            {
                isAuthorized = true;
            }

            if (!isAuthorized && request.IsSpecialist && attachment.DeliveryId.HasValue)
            {
                var deliveryId = attachment.DeliveryId.Value;
                var isAssignedToReview = await _context.ContractSpecialistReviews
                    .AnyAsync(r => r.DeliveryId == deliveryId && r.AssignedSpecialistId == request.RequestingUserId, cancellationToken);
                var isAssignedToRevision = await _context.RevisionRequests
                    .AnyAsync(r => r.DeliveryId == deliveryId && r.SpecialistId == request.RequestingUserId, cancellationToken);

                if (isAssignedToReview || isAssignedToRevision)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You do not have access to this file."
                };
            }

            var physicalPath = _fileStorage.GetPhysicalPath(attachment.FileUrl);
            if (physicalPath == null)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FileNotFound,
                    Message = "The file could not be found on the server."
                };
            }

            return new Result<DownloadFileResult>
            {
                Succeeded = true,
                Data = new DownloadFileResult
                {
                    PhysicalPath = physicalPath,
                    OriginalFileName = attachment.OriginalFileName,
                    ContentType = GetContentType(attachment.FileType)
                }
            };
        }

        private static string GetContentType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".pdf": return "application/pdf";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".mp4": return "video/mp4";
                case ".mov": return "video/quicktime";
                case ".mp3": return "audio/mpeg";
                case ".wav": return "audio/wav";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xls": return "application/vnd.ms-excel";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".doc": return "application/msword";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".zip": return "application/zip";
                case ".rar": return "application/vnd.rar";
                case ".7z": return "application/x-7z-compressed";
                case ".tar": return "application/x-tar";
                case ".txt": return "text/plain";
                case ".csv": return "text/csv";
                default: return "application/octet-stream";
            }
        }
    }
}
