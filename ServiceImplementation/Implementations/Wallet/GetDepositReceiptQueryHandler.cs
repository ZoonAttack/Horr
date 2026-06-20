using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Responses;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.Storage;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Wallet
{
    public class GetDepositReceiptQueryHandler : IRequestHandler<GetDepositReceiptQuery, Result<DownloadFileResult>>
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public GetDepositReceiptQueryHandler(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<DownloadFileResult>> Handle(GetDepositReceiptQuery request, CancellationToken cancellationToken)
        {
            var idString = request.Id.ToString();
            var depositRequest = await _context.DepositRequests
                .FirstOrDefaultAsync(dr => dr.Id == idString, cancellationToken);

            if (depositRequest == null)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.DepositRequestNotFound,
                    Message = "Deposit request not found."
                };
            }

            if (!request.IsAdmin && depositRequest.ClientId != request.RequestingUserId)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You do not have access to this receipt."
                };
            }

            var physicalPath = _fileStorage.GetPhysicalPath(depositRequest.ReceiptPhotoUrl);
            if (physicalPath == null)
            {
                return new Result<DownloadFileResult>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FileNotFound,
                    Message = "The receipt file could not be found on the server."
                };
            }

            var extension = Path.GetExtension(physicalPath);
            return new Result<DownloadFileResult>
            {
                Succeeded = true,
                Data = new DownloadFileResult
                {
                    PhysicalPath = physicalPath,
                    OriginalFileName = $"receipt_{request.Id}{extension}",
                    ContentType = GetContentType(extension)
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
                default: return "application/octet-stream";
            }
        }
    }
}
