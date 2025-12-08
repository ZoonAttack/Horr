using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet
{
    public static class WalletExtensions
    {
        /// <summary>
        /// Converts Wallet entity to WalletReadDTO
        /// </summary>
        public static WalletReadDTO Wallet_To_WalletRead(this Entities.Payment.Wallet wallet)
        {
            if (wallet == null)
            {
                return null;
            }

            return new WalletReadDTO
            {
                Id = wallet.Id.ToString(),
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt
            };
        }

        /// <summary>
        /// Converts WalletCreateDTO to Wallet entity
        /// </summary>
        public static Entities.Payment.Wallet WalletCreate_To_Wallet(this WalletCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Payment.Wallet
            {
                UserId = createDto.UserId,
                Balance = 0
            };
        }

        /// <summary>
        /// Applies WalletUpdateDTO to an existing Wallet entity
        /// </summary>
        public static void WalletUpdate_To_Wallet(this Entities.Payment.Wallet wallet, WalletUpdateDTO updateDto)
        {
            if (wallet == null || updateDto == null)
            {
                return;
            }

            wallet.Balance = updateDto.Balance;
        }
    }
}
