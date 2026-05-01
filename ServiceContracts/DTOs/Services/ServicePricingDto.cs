namespace ServiceContracts.DTOs.Services
{
    public class ServicePricingDto
    {
        public string Id { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public int DeliveryDays { get; set; }
        public int RevisionsIncluded { get; set; }
    }
}
