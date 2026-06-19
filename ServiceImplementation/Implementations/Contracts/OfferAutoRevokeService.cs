using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using Entities.Enums;
using Entities.Payment;
using Services.Wallet;

namespace ServiceImplementation.Implementations.Contracts
{
    public class OfferAutoRevokeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OfferAutoRevokeService> _logger;

        public OfferAutoRevokeService(IServiceProvider serviceProvider, ILogger<OfferAutoRevokeService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Offer Auto-Revocation Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredOffersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ProcessExpiredOffersAsync.");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Offer Auto-Revocation Service is stopping.");
        }

        public async Task ProcessExpiredOffersAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var escrowService = scope.ServiceProvider.GetRequiredService<IEscrowService>();

                var limitTime = DateTime.UtcNow.AddDays(-3);

                var expiredOffers = await context.Contracts
                    .Include(c => c.Proposal)
                    .Where(c => c.Status == ContractStatus.Draft && c.CreatedAt <= limitTime)
                    .ToListAsync(cancellationToken);

                if (expiredOffers.Any())
                {
                    _logger.LogInformation($"Found {expiredOffers.Count} draft contract offers older than 3 days.");

                    foreach (var contract in expiredOffers)
                    {
                        try
                        {
                            // Refund Escrowed Funds to Client via EscrowService
                            var milestone = await context.ContractMilestones
                                .FirstOrDefaultAsync(m => m.ContractId == contract.Id, cancellationToken);

                            if (milestone != null)
                            {
                                var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");
                                await escrowService.RefundToClientAsync(contractGuid, milestone.Id, "Offer expired and auto-revoked");
                            }
                            else
                            {
                                // Fallback
                                var clientWallet = await context.WalletBalances
                                    .FirstOrDefaultAsync(w => w.UserId == contract.ClientId, cancellationToken);
                                if (clientWallet != null)
                                {
                                    clientWallet.BalanceEGP += contract.AgreedRate;
                                    clientWallet.LastUpdatedAt = DateTime.UtcNow;

                                    var transaction = new Transaction
                                    {
                                        UserId = contract.ClientId,
                                        Amount = contract.AgreedRate,
                                        Direction = TransactionDirection.Credit,
                                        Type = TransactionType.Refund,
                                        Description = $"Refund of escrowed funds for expired offer (Contract ID: {contract.Id})",
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    context.Transactions.Add(transaction);
                                }
                            }

                            if (contract.Proposal != null)
                            {
                                contract.Proposal.Status = ProposalStatus.Rejected;
                            }

                            contract.Status = ContractStatus.Closed;
                            contract.ClosedAt = DateTime.UtcNow;

                            _logger.LogInformation($"Auto-revoked expired offer Contract #{contract.Id} for Proposal #{contract.ProposalId}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to auto-revoke expired offer Contract {contract.Id}");
                        }
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}