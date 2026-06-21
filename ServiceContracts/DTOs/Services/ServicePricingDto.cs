namespace ServiceContracts.DTOs.Services
{
    public class ServicePricingDto
    {
        public string? Id { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public string? OriginalCurrency { get; set; }
        public decimal? ConvertedPriceFrom { get; set; }
        public decimal? ConvertedPriceTo { get; set; }
        public string? ConvertedCurrency { get; set; }
        public int? DeliveryDays { get; set; }
        public int? RevisionsIncluded { get; set; }
    }
}
