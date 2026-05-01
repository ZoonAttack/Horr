using MediatR;
using Entities;
using Entities.Payment;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Exceptions;
using ServiceImplementation.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ServiceImplementation.Implementations.Wallet
{
    public class SubmitDepositRequestCommandHandler : IRequestHandler<SubmitDepositRequestCommand, DepositRequestDto>
    {
        private readonly AppDbContext _context;

        public SubmitDepositRequestCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DepositRequestDto> Handle(SubmitDepositRequestCommand request, CancellationToken cancellationToken)
        {
            Validate(request);

            // Mock file upload since no existing convention was found
            // In a real scenario, this would use a dedicated IFileService
            string photoUrl = await MockUploadAsync(request.ReceiptPhoto!);

            var depositRequest = new DepositRequest
            {
                ClientId = request.ClientId,
                Amount = request.Amount,
                ReceiptNumber = request.ReceiptNumber,
                ReceiptPhotoUrl = photoUrl,
                Status = DepositStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.DepositRequests.Add(depositRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return depositRequest.ToDto();
        }

        private void Validate(SubmitDepositRequestCommand request)
        {
            var errors = new List<string>();

            if (request.Amount <= 0)
            {
                errors.Add("Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
            {
                errors.Add("Receipt number is required.");
            }

            if (request.ReceiptPhoto == null)
            {
                errors.Add("Receipt photo is required.");
            }

            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }
        }

        private async Task<string> MockUploadAsync(IFormFile file)
        {
            // Simulating a file upload process
            await Task.Delay(10); 
            return $"/uploads/receipts/{Guid.NewGuid()}_{file.FileName}";
        }
    }
}
