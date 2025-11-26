using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Wallet.Funds
{
    public class FundRequestCreateDTO
    {
        public long PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string ClientTransactionReference { get; set; }
    }
}
