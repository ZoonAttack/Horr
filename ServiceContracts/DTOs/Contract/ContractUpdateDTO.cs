using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    /// <summary>
    /// DTO for updating existing Contract details.
    /// </summary>
    public class ContractUpdateDTO
    {
        public ContractStatus Status { get; set; }

        public decimal? AgreedRate { get; set; }
    }
}
