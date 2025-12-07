using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    /// <summary>
    /// DTO for updating existing Contract details.
    /// </summary>
    public class ContractUpdateDTO
    {
        public string Terms { get; set; }

        public ContractStatus Status { get; set; }
    }
}
