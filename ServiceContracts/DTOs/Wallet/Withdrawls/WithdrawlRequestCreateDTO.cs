using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Wallet.Withdrawls
{
    public class WithdrawalRequestCreateDTO
    {
        public string PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
    }
}
