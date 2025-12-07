using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Wallet
{
    /// <summary>
    /// DTO for updating Wallet balance (typically through transactions).
    /// </summary>
    public class WalletUpdateDTO
    {
        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Balance { get; set; }
    }
}
