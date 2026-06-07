using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ServiceImplementation.Implementations.Wallet
{
    public class SubmitDepositRequestCommandHandler : IRequestHandler<SubmitDepositRequestCommand, Result<DepositRequestDto>>
    {
        private readonly AppDbContext _context;

        public SubmitDepositRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DepositRequestDto>> Handle(SubmitDepositRequestCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<DepositRequestDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Client account not found or is deleted."
                };
            }

            var validationResult = Validate(request);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            // Mock file upload since no existing convention was found
            // In a real scenario, this would use a dedicated IFileService
            string photoUrl = await MockUploadAsync(request.ReceiptPhoto!);

            var depositRequest = new DepositRequest
            {
                ClientId = request.ClientId!,
                Amount = request.Amount,
                ReceiptNumber = request.ReceiptNumber,
                ReceiptPhotoUrl = photoUrl,
                Status = DepositStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.DepositRequests.Add(depositRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result<DepositRequestDto>
            {
                Succeeded = true,
                Data = depositRequest.ToDto()
            };
        }

        private Result<DepositRequestDto> Validate(SubmitDepositRequestCommand request)
        {
            if (request.Amount <= 0)
            {
                return new Result<DepositRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.InvalidAmount, Message = "Amount must be greater than zero." };
            }

            if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
            {
                return new Result<DepositRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.MissingPaymentDetails, Message = "Receipt number is required." };
            }

            if (request.ReceiptPhoto == null)
            {
                return new Result<DepositRequestDto> { Succeeded = false, ErrorCode = ErrorCodes.MissingPaymentDetails, Message = "Receipt photo is required." };
            }

            return new Result<DepositRequestDto> { Succeeded = true };
        }

        private async Task<string> MockUploadAsync(IFormFile file)
        {
            // Simulating a file upload process
            await Task.Delay(10); 
            return $"/uploads/receipts/{Guid.NewGuid()}_{file.FileName}";
        }
    }
}
