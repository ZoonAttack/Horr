using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Wallet
{
    /// <summary>
    /// DTO for creating a new Wallet.
    /// </summary>
    public class WalletCreateDTO
    {
        [Required]
        public long UserId { get; set; }
    }
}
