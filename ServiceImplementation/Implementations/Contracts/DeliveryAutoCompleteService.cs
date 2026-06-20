using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Enums;
using Services.Wallet;

namespace ServiceImplementation.Implementations.Contracts
{
    public class DeliveryAutoCompleteService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeliveryAutoCompleteService> _logger;

        public DeliveryAutoCompleteService(IServiceProvider serviceProvider, ILogger<DeliveryAutoCompleteService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Delivery Auto-Complete Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingDeliveriesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ProcessPendingDeliveriesAsync.");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Delivery Auto-Complete Service is stopping.");
        }

        public async Task ProcessPendingDeliveriesAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var escrowService = scope.ServiceProvider.GetRequiredService<IEscrowService>();

                var now = DateTime.UtcNow;

                var expiredDeliveries = await context.ContractDeliveries
                    .Where(d => d.Status == DeliveryStatus.Pending && d.ReviewDeadline <= now)
                    .ToListAsync(cancellationToken);

                if (expiredDeliveries.Any())
                {
                    _logger.LogInformation($"Found {expiredDeliveries.Count} pending deliveries past review deadline.");

                    foreach (var delivery in expiredDeliveries)
                    {
                        try
                        {
                            var hasDispute = await context.Disputes.AnyAsync(
                                d => d.ContractDeliveryId == delivery.Id && 
                                (d.Status == DisputeStatus.Open || d.Status == DisputeStatus.UnderReview), 
                                cancellationToken);

                            var hasActiveRevision = await context.RevisionRequests.AnyAsync(
                                r => r.DeliveryId == delivery.Id && 
                                (r.Status == RevisionStatus.Pending || r.Status == RevisionStatus.AcceptedBySpecialist), 
                                cancellationToken);

                            var hasActiveSpecialistReview = await context.ContractSpecialistReviews.AnyAsync(
                                r => r.DeliveryId == delivery.Id && 
                                (r.Status == SpecialistReviewStatus.Pending || r.Status == SpecialistReviewStatus.InProgress), 
                                cancellationToken);

                            if (hasDispute || hasActiveRevision || hasActiveSpecialistReview)
                            {
                                _logger.LogInformation($"Skipping auto-approval for delivery {delivery.Id} due to active blocker (Dispute: {hasDispute}, Revision: {hasActiveRevision}, Review: {hasActiveSpecialistReview})");
                                continue;
                            }

                            var contract = await context.Contracts
                                .FirstOrDefaultAsync(c => c.Id == delivery.ContractId, cancellationToken);

                            if (contract != null)
                            {
                                delivery.Status = DeliveryStatus.Approved;
                                delivery.CompletedAt = now;

                                var contractGuid = Guid.Parse($"00000000-0000-0000-0000-{contract.Id:x12}");

                                // Call escrow service to release funds to freelancer
                                await escrowService.ReleaseToFreelancerAsync(contractGuid, delivery.ContractMilestoneId);

                                contract.Status = ContractStatus.Completed;
                                contract.ClosedAt = now;

                                _logger.LogInformation($"Auto-approved delivery {delivery.Id} for Contract #{contract.Id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to auto-approve delivery {delivery.Id}");
                        }
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
