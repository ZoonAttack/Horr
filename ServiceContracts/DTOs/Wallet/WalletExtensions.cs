using Entities.Payment;

namespace ServiceContracts.DTOs.Wallet
{
    public static class WalletExtensions
    {
        /// <summary>
        /// Converts WalletBalance entity to WalletReadDTO
        /// </summary>
        public static WalletReadDTO Wallet_To_WalletRead(this Entities.Payment.WalletBalance wallet)
        {
            if (wallet == null)
            {
                return null;
            }

            return new WalletReadDTO
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.BalanceEGP,
                UpdatedAt = wallet.LastUpdatedAt
            };
        }

        /// <summary>
        /// Converts WalletCreateDTO to WalletBalance entity
        /// </summary>
        public static Entities.Payment.WalletBalance WalletCreate_To_Wallet(this WalletCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Payment.WalletBalance
            {
                UserId = createDto.UserId,
                BalanceEGP = 0
            };
        }

        /// <summary>
        /// Applies WalletUpdateDTO to an existing WalletBalance entity
        /// </summary>
        public static void WalletUpdate_To_Wallet(this Entities.Payment.WalletBalance wallet, WalletUpdateDTO updateDto)
        {
            if (wallet == null || updateDto == null)
            {
                return;
            }

            wallet.BalanceEGP = updateDto.Balance;
        }
    }
}
