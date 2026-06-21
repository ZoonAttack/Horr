using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using ServiceContracts.DTOs.Services;
using ServiceContracts.Currency;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class GetMyServicesQueryHandler : IRequestHandler<GetMyServicesQuery, ServiceGroupedDto>
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyConverterService _currencyConverter;

        public GetMyServicesQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<ServiceGroupedDto> Handle(GetMyServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.GalleryFiles)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .Include(s => s.Freelancer)
                .Where(s => s.FreelancerId == request.FreelancerId && !s.IsDeleted)
                .ToListAsync(cancellationToken);

            // Viewer is the freelancer themselves
            var user = await _context.Users.FindAsync(request.FreelancerId);
            string targetCurrency = user?.PreferredCurrency ?? "USD";

            var result = new ServiceGroupedDto();
            result.Approved = new List<ServiceCatalogItemDto>();
            result.UnderReview = new List<ServiceCatalogItemDto>();

            string baseCurrency = user?.PreferredCurrency ?? "USD";

            foreach (var s in services)
            {
                var dto = s.ToDto();
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

                if (s.Status == ServiceStatus.Approved)
                {
                    result.Approved.Add(dto);
                }
                else if (s.Status == ServiceStatus.UnderReview)
                {
                    result.UnderReview.Add(dto);
                }
            }

            return result;
        }
    }
}
