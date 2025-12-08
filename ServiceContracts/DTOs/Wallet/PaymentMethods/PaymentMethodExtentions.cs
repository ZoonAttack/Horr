using Entities.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Wallet.PaymentMethods
{
    public static class PaymentMethodExtensions
    {
        public static PaymentMethodReadDTO ToPaymentMethodRead(this PaymentMethod method)
        {
            if (method == null) return null;

            return new PaymentMethodReadDTO
            {
                Id = method.Id.ToString(),
                UserId = method.UserId,
                MethodName = method.MethodName,
                AccountIdentifier = method.AccountIdentifier,
                CreatedAt = method.CreatedAt
            };
        }

        public static PaymentMethod ToPaymentMethod(this PaymentMethodCreateDTO createDto, string userId)
        {
            if (createDto == null) return null;

            return new PaymentMethod
            {
                UserId = userId,
                MethodName = createDto.MethodName.ToString(),
                AccountIdentifier = createDto.AccountIdentifier,
            };
        }
    }
}
