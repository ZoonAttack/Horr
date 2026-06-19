using Entities;
using Entities.Enums;
using Entities.Project;
using Entities.Payment;
using FluentAssertions;
using Horr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts.DTOs.Contract;
using ServiceImplementation.Implementations.Contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting.Integration
{
    public class ContractEscrowEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public ContractEscrowEndpointsTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<(Guid clientGuid, Guid freelancerGuid, Contract contract, ContractMilestone milestone)> PrepareDataAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var clientGuid = Guid.NewGuid();
            var freelancerGuid = Guid.NewGuid();

            var client = new Entities.Users.User
            {
                Id = clientGuid.ToString(),
                UserName = "client",
                Email = "client@test.com",
                FullName = "Client User",
                Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC"
            };

            var freelancer = new Entities.Users.User
            {
                Id = freelancerGuid.ToString(),
                UserName = "freelancer",
                Email = "freelancer@test.com",
                FullName = "Freelancer User",
                Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC"
            };

            var specialist = new Entities.Users.User
            {
                Id = "specialist-id",
                UserName = "specialist",
                Email = "specialist@test.com",
                FullName = "Specialist User",
                Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC"
            };

            db.Users.Add(client);
            db.Users.Add(freelancer);
            db.Users.Add(specialist);

            var contract = new Contract
            {
                Id = 15,
                ClientId = clientGuid.ToString(),
                FreelancerId = freelancerGuid.ToString(),
                AgreedRate = 1000m,
                Status = ContractStatus.Active,
                MaxRevisions = 3,
                CreatedAt = DateTime.UtcNow
            };
            db.Contracts.Add(contract);

            var milestone = new ContractMilestone
            {
                Id = Guid.NewGuid(),
                ContractId = 15,
                Title = "First Milestone",
                Amount = 400m,
                Status = MilestoneStatus.Unfunded
            };
            db.ContractMilestones.Add(milestone);

            // Add client wallet balance
            db.WalletBalances.Add(new WalletBalance { UserId = clientGuid.ToString(), BalanceEGP = 1500m, LastUpdatedAt = DateTime.UtcNow });
            db.WalletBalances.Add(new WalletBalance { UserId = freelancerGuid.ToString(), BalanceEGP = 0m, LastUpdatedAt = DateTime.UtcNow });

            await db.SaveChangesAsync();
            return (clientGuid, freelancerGuid, contract, milestone);
        }

        [Fact]
        public async Task SubmitDelivery_FreelancerRole_Returns201()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            // Setup Held escrow first so submission is allowed
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Amount = milestone.Amount,
                    PlatformFeeFromClient = milestone.Amount * 0.055m,
                    PlatformFeeFromFreelancer = milestone.Amount * 0.15m,
                    NetToFreelancer = milestone.Amount * 0.85m,
                    Status = EscrowStatus.Held,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var request = new
            {
                ContractId = contract.Id,
                ContractMilestoneId = milestone.Id,
                DeliveryNote = "Here is the completed code.",
                Attachments = new List<AttachmentDto>
                {
                    new AttachmentDto { Type = AttachmentType.Link, Url = "https://github.com/test/repo" }
                }
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", freelancerGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Freelancer");

            var response = await _client.PostAsJsonAsync("/api/deliveries/submit", request, _jsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<ContractDeliveryDto>(_jsonOptions);
            result.Should().NotBeNull();
            result!.ContractId.Should().Be(contract.Id);
            result.Status.Should().Be(DeliveryStatus.Pending);
            result.Attachments.Should().HaveCount(1);
        }

        [Fact]
        public async Task SubmitDelivery_NonFreelancer_Returns403()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            var request = new
            {
                ContractId = contract.Id,
                ContractMilestoneId = milestone.Id,
                DeliveryNote = "Should fail",
                Attachments = new List<AttachmentDto>()
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.PostAsJsonAsync("/api/deliveries/submit", request, _jsonOptions);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task FundMilestone_ValidClient_Returns200()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.PostAsync($"/api/milestones/{milestone.Id}/fund", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var balance = await db.WalletBalances.FirstAsync(w => w.UserId == clientGuid.ToString());
                // 1500 - (400 + 5.5% fee) = 1500 - 422 = 1078m
                balance.BalanceEGP.Should().Be(1078m);

                var milestoneDb = await db.ContractMilestones.FindAsync(milestone.Id);
                milestoneDb!.Status.Should().Be(MilestoneStatus.Funded);
            }
        }

        [Fact]
        public async Task ApproveDelivery_ValidClient_ReleasesEscrow_Returns200()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            Guid deliveryId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Add Held escrow with proper NetToFreelancer splits
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Amount = milestone.Amount,
                    PlatformFeeFromClient = milestone.Amount * 0.055m,
                    PlatformFeeFromFreelancer = milestone.Amount * 0.15m,
                    NetToFreelancer = milestone.Amount * 0.85m, // 340m
                    Status = EscrowStatus.Held,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });

                var delivery = new ContractDelivery
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Status = DeliveryStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    ReviewDeadline = DateTime.UtcNow.AddDays(3)
                };
                db.ContractDeliveries.Add(delivery);
                await db.SaveChangesAsync();
                deliveryId = delivery.Id;
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.PostAsync($"/api/deliveries/{deliveryId}/approve", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Freelancer receives payout minus 15% commission = 400 * 0.85 = 340m
                var freeBalance = await db.WalletBalances.FirstAsync(w => w.UserId == freelancerGuid.ToString());
                freeBalance.BalanceEGP.Should().Be(340m);

                var deliveryDb = await db.ContractDeliveries.FindAsync(deliveryId);
                deliveryDb!.Status.Should().Be(DeliveryStatus.Approved);
            }
        }

        [Fact]
        public async Task RequestRevision_ValidClient_Returns201()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            Guid deliveryId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var delivery = new ContractDelivery
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Status = DeliveryStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    ReviewDeadline = DateTime.UtcNow.AddDays(3)
                };
                db.ContractDeliveries.Add(delivery);
                await db.SaveChangesAsync();
                deliveryId = delivery.Id;
            }

            var request = new { Reason = "Please revise page 2." };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/revision", request, _jsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<RevisionRequestDto>(_jsonOptions);
            result.Should().NotBeNull();
            result!.DeliveryId.Should().Be(deliveryId);
            result.Reason.Should().Be("Please revise page 2.");
        }

        [Fact]
        public async Task OpenDispute_ValidUser_Returns201()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            Guid deliveryId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var delivery = new ContractDelivery
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Status = DeliveryStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    ReviewDeadline = DateTime.UtcNow.AddDays(3)
                };
                db.ContractDeliveries.Add(delivery);
                await db.SaveChangesAsync();
                deliveryId = delivery.Id;
            }

            var request = new { ContractId = contract.Id, Reason = "Delayed work." };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", freelancerGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Freelancer");

            var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/dispute", request, _jsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<DisputeDto>(_jsonOptions);
            result.Should().NotBeNull();
            result!.ContractDeliveryId.Should().Be(deliveryId);
            result.Status.Should().Be(DisputeStatus.Open);
        }

        [Fact]
        public async Task ResolveDispute_AdminOnly_Returns200()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            Guid disputeId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // 1. Seed Held Escrow Transaction
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Amount = milestone.Amount,
                    PlatformFeeFromClient = milestone.Amount * 0.055m,
                    PlatformFeeFromFreelancer = milestone.Amount * 0.15m,
                    NetToFreelancer = milestone.Amount * 0.85m, // 340m
                    Status = EscrowStatus.Held,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });

                // 2. Seed Contract Delivery
                var delivery = new ContractDelivery
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractMilestoneId = milestone.Id,
                    Status = DeliveryStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    ReviewDeadline = DateTime.UtcNow.AddDays(3)
                };
                db.ContractDeliveries.Add(delivery);

                // 3. Seed Dispute linked to delivery
                var dispute = new Dispute
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ContractDeliveryId = delivery.Id,
                    OpenedByUserId = freelancerGuid.ToString(),
                    Reason = "Disagreement",
                    Status = DisputeStatus.Open,
                    OpenedAt = DateTime.UtcNow
                };
                db.Disputes.Add(dispute);
                await db.SaveChangesAsync();
                disputeId = dispute.Id;
            }

            var request = new { Decision = DisputeDecision.ForFreelancer, AdminDecision = "Freelancer wins." };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "admin-id");
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Admin");

            var response = await _client.PostAsJsonAsync($"/api/disputes/{disputeId}/resolve", request, _jsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var freeBalance = await db.WalletBalances.FirstAsync(w => w.UserId == freelancerGuid.ToString());
                freeBalance.BalanceEGP.Should().Be(340m); // Payout min commission (400 * 0.85 = 340)

                var disputeDb = await db.Disputes.FindAsync(disputeId);
                disputeDb!.Status.Should().Be(DisputeStatus.ResolvedForFreelancer);
            }
        }

        [Fact]
        public async Task GetContractDeliveries_ClientRole_Returns200()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.GetAsync($"/api/deliveries?contractId={contract.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<ContractDeliveryDto>>(_jsonOptions);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEscrowSummary_ClientRole_Returns200_WithCorrectMath()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Held = 100, Released = 200, Refunded = 100 (TotalFunded = 400)
                
                // 1. Released Milestone (Milestone 1 - 200 EGP)
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Amount = 200m,
                    PlatformFeeFromClient = 11m,
                    PlatformFeeFromFreelancer = 30m,
                    NetToFreelancer = 170m,
                    Status = EscrowStatus.Released,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Amount = 200m,
                    NetToFreelancer = 170m,
                    Status = EscrowStatus.Released,
                    Type = EscrowTransactionType.ReleasedToFreelancer,
                    CreatedAt = DateTime.UtcNow
                });

                // 2. Refunded Milestone (Milestone 2 - 100 EGP)
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Amount = 100m,
                    PlatformFeeFromClient = 5.5m,
                    PlatformFeeFromFreelancer = 15m,
                    NetToFreelancer = 85m,
                    Status = EscrowStatus.Refunded,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Amount = 100m,
                    Status = EscrowStatus.Refunded,
                    Type = EscrowTransactionType.RefundedToClient,
                    CreatedAt = DateTime.UtcNow
                });

                // 3. Held Milestone (Milestone 3 - 100 EGP)
                db.EscrowTransactions.Add(new EscrowTransaction
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Amount = 100m,
                    PlatformFeeFromClient = 5.5m,
                    PlatformFeeFromFreelancer = 15m,
                    NetToFreelancer = 85m,
                    Status = EscrowStatus.Held,
                    Type = EscrowTransactionType.ClientFunded,
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", clientGuid.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.GetAsync($"/api/contracts/{contract.Id}/escrow");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<EscrowSummaryDto>(_jsonOptions);
            result.Should().NotBeNull();
            result!.CurrentlyHeld.Should().Be(100m); // Held of ClientFunded = 100
            result.TotalFunded.Should().Be(400m); // ClientFunded = 200 + 100 + 100 = 400
            result.TotalReleased.Should().Be(170m); // ReleasedToFreelancer NetToFreelancer = 170
            result.TotalRefunded.Should().Be(100m); // RefundedToClient Amount = 100
        }

        [Fact]
        public async Task GetOpenRevisions_SpecialistRole_Returns200()
        {
            var (clientGuid, freelancerGuid, contract, milestone) = await PrepareDataAsync();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.RevisionRequests.Add(new RevisionRequest
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = Guid.NewGuid(),
                    RequestedByClientId = clientGuid.ToString(),
                    Reason = "Reason",
                    Status = RevisionStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "specialist-id");
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Specialist");

            var response = await _client.GetAsync("/api/revisions/open");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<RevisionRequestDto>>(_jsonOptions);
            result.Should().HaveCount(1);
        }
    }
}
