using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using ServiceContracts.DTOs.Services;
using ServiceContracts.Currency;
using ServiceImplementation.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceCatalogItemDto>
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyConverterService _currencyConverter;

        public GetServiceByIdQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<ServiceCatalogItemDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.GalleryFiles)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .Include(s => s.Freelancer)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

            if (service == null)
            {
                throw new NotFoundException($"Service with ID {request.Id} not found.");
            }

            if (service.FreelancerId != request.FreelancerId)
            {
                // To the user, it should look like it doesn't exist or is inaccessible
                throw new NotFoundException($"Service with ID {request.Id} not found.");
            }

            var dto = service.ToDto();
            
            // Assuming the viewer's preferred currency is needed.
            // request.FreelancerId here is actually the ViewerId based on ServicesController.GetById
            var viewer = await _context.Users.FindAsync(request.FreelancerId);
            string targetCurrency = viewer?.PreferredCurrency ?? "USD";
            string baseCurrency = service.Freelancer?.User?.PreferredCurrency ?? "USD";

            dto.OriginalCurrency = baseCurrency;
            dto.ConvertedCurrency = targetCurrency;

            if (string.Equals(baseCurrency, targetCurrency, System.StringComparison.OrdinalIgnoreCase))
            {
                dto.ConvertedPrice = dto.Price;
                if (dto.Pricing != null)
                {
                    dto.Pricing.OriginalCurrency = baseCurrency;
                    dto.Pricing.ConvertedCurrency = targetCurrency;
                    dto.Pricing.ConvertedPriceFrom = dto.Pricing.PriceFrom;
                    dto.Pricing.ConvertedPriceTo = dto.Pricing.PriceTo;
                }
            }
            else
            {
                try
                {
                    dto.ConvertedPrice = await _currencyConverter.ConvertAsync(dto.Price ?? 0, baseCurrency, targetCurrency);
                    
                    if (dto.Pricing != null)
                    {
                        dto.Pricing.OriginalCurrency = baseCurrency;
                        dto.Pricing.ConvertedCurrency = targetCurrency;
                        dto.Pricing.ConvertedPriceFrom = await _currencyConverter.ConvertAsync(dto.Pricing.PriceFrom ?? 0, baseCurrency, targetCurrency);
                        dto.Pricing.ConvertedPriceTo = await _currencyConverter.ConvertAsync(dto.Pricing.PriceTo ?? 0, baseCurrency, targetCurrency);
                    }
                }
                catch
                {
                    dto.ConvertedPrice = dto.Price;
                    dto.ConvertedCurrency = baseCurrency;
                    if (dto.Pricing != null)
                    {
                        dto.Pricing.ConvertedCurrency = baseCurrency;
                        dto.Pricing.ConvertedPriceFrom = dto.Pricing.PriceFrom;
                        dto.Pricing.ConvertedPriceTo = dto.Pricing.PriceTo;
                    }
                }
            }

            return dto;
        }
    }
}
