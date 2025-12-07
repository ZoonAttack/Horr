using System;
using System.Collections.Generic;
using Entities.Enums;
using Entities.Payment;
using ServiceContracts.DTOs.Wallet.PaymentTransaction;

namespace ServiceContracts.DTOs.Wallet.Payment
{
    public class PaymentCreateDTO
    {
        // Links to business context
        public long ProjectId { get; set; }
        public long? FreelancerId { get; set; }

        // Financial Details
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal? PlatformCommission { get; set; }
    }

    public class PaymentReadDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long? FreelancerId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal? PlatformCommission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        // Linked Transaction IDs from the junction table
        public List<PaymentTransactionReadDTO> Transactions { get; set; } = new List<PaymentTransactionReadDTO>();
    }

    public class PaymentUpdateDTO
    {
        public PaymentStatus Status { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }

    public static class PaymentExtensions
    {
        public static PaymentReadDTO ToPaymentRead(this Entities.Payment.Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            return new PaymentReadDTO
            {
                Id = payment.Id,
                ProjectId = payment.ProjectId,
                FreelancerId = payment.FreelancerId,
                Amount = payment.Amount,
                PaymentType = payment.PaymentType,
                Status = payment.Status,
                PlatformCommission = payment.PlatformCommission,
                CreatedAt = payment.CreatedAt,
                ReleasedAt = payment.ReleasedAt,

                // Map junction table links using the specific extension method
                Transactions = payment.PaymentTransactions
                    .Select(pt => pt.ToPaymentTransactionRead())
                    .ToList()
            };
        }

        public static Entities.Payment.Payment ToPayment(this PaymentCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Payment.Payment
            {
                ProjectId = createDto.ProjectId,
                FreelancerId = createDto.FreelancerId,
                Amount = createDto.Amount,
                PaymentType = createDto.PaymentType,
                PlatformCommission = createDto.PlatformCommission,
            };
        }

        public static void ApplyUpdate(this Entities.Payment.Payment payment, PaymentUpdateDTO updateDto)
        {
            if (payment == null || updateDto == null)
            {
                return;
            }

            // Only update mutable fields
            payment.Status = updateDto.Status;
            payment.ReleasedAt = updateDto.ReleasedAt;
        }
    }
}