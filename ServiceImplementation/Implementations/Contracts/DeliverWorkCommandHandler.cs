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

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeliverWorkCommandHandler : IRequestHandler<DeliverWorkCommand, Result<WorkDeliveryDto>>
    {
        private readonly AppDbContext _context;

        public DeliverWorkCommandHandler(AppDbContext context)
        {
            _context = context;
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
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "deliveries");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in request.Files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream, cancellationToken);
                        }

                        // Entity only has FileUrl and FileType
                        var attachment = new DeliveryAttachment
                        {
                            WorkDelivery = delivery,
                            FileUrl = $"/uploads/deliveries/{fileName}",
                            FileType = Path.GetExtension(file.FileName)
                        };
                        _context.DeliveryAttachments.Add(attachment);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new Result<WorkDeliveryDto>
            {
                Succeeded = true,
                Data = new WorkDeliveryDto
                {
                    Id = delivery.Id,
                    ContractId = delivery.ContractId,
                    Note = delivery.Note,
                    ActionStatus = delivery.ActionStatus,
                    SubmittedAt = delivery.SubmittedAt
                }
            };
        }
    }
}
