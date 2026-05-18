namespace ServiceContracts.DTOs.Contract
{
    public class EscrowSummaryDto
    {
        public decimal TotalFunded { get; set; }
        public decimal TotalReleased { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal PlatformEarned { get; set; }
        public decimal CurrentlyHeld { get; set; }
    }
}
