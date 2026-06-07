using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Exceptions;
using ServiceContracts.Storage;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeliverWorkCommandHandler : IRequestHandler<DeliverWorkCommand, Result<WorkDeliveryDto>>
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DeliverWorkCommandHandler(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<WorkDeliveryDto>> Handle(DeliverWorkCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<WorkDeliveryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Freelancer account not found or is deleted."
                };
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                return new Result<WorkDeliveryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.ContractNotFound,
                    Message = $"Contract with ID {request.ContractId} not found."
                };
            }

            var errors = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(request.Note))
            {
                errors.Add("Note is required.");
            }
            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }

            if (contract.FreelancerId != request.FreelancerId)
            {
                return new Result<WorkDeliveryDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "Unauthorized: Only the contract freelancer can deliver work."
                };
            }

            // State Guard
            ContractStateGuard.EnsureCanDeliverWork(contract);

            // Create delivery
            var delivery = new WorkDelivery
            {
                ContractId = contract.Id,
                Note = request.Note,
                SubmittedAt = DateTime.UtcNow,
                ActionStatus = ActionStatus.NeedsAttention
            };

            _context.WorkDeliveries.Add(delivery);

            // Handle file uploads
            if (request.Files != null && request.Files.Count > 0)
            {
                foreach (var file in request.Files)
                {
                    if (file.Length > 0)
                    {
                        var storedFile = await _fileStorage.SaveAsync(file, "deliveries", cancellationToken);

                        var attachment = new DeliveryAttachment
                        {
                            WorkDelivery = delivery,
                            FileUrl = storedFile.FileUrl,
                            OriginalFileName = storedFile.OriginalFileName,
                            FileType = storedFile.FileType,
                            FileSizeBytes = storedFile.FileSizeBytes
                        };
                        _context.DeliveryAttachments.Add(attachment);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var dto = delivery.ToDto();
            if (dto.Attachments == null || dto.Attachments.Count == 0)
            {
                dto.Attachments = delivery.Attachments?.Select(a => a.ToDto()).ToList() ?? new System.Collections.Generic.List<AttachmentDto>();
            }

            return new Result<WorkDeliveryDto>
            {
                Succeeded = true,
                Data = dto
            };
        }
    }
}
