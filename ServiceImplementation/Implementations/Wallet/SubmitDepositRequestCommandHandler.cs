using MediatR;
using Entities;
using ServiceContracts.DTOs.Wallet;
using ServiceImplementation.Exceptions;
using Microsoft.EntityFrameworkCore;

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

            // Implementation will follow in next subtasks
            throw new NotImplementedException();
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
    }
}
