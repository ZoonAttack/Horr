using Entities.Enums;
using FluentAssertions;
using Horr;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts.DTOs.Wallet;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Entities.Payment;
using Entities.Users;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace UnitTesting.Integration
{
    public class EndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EndpointTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task PrepareDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();


            // Seed a user
            var user = new Entities.Users.User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@test.com",
                FullName = "Test User",
                Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC"
            };
            db.Users.Add(user);
            
            var admin = new Entities.Users.User
            {
                Id = "admin-id",
                UserName = "admin",
                Email = "admin@test.com",
                FullName = "Admin User",
                Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC"
            };
            db.Users.Add(admin);

            db.WalletBalances.Add(new WalletBalance { UserId = "test-user-id", BalanceEGP = 1000 });
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task SubmitDeposit_ValidPayload_Returns201()
        {
            await PrepareDatabaseAsync();
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("500"), "Amount");
            content.Add(new StringContent("REC123"), "ReceiptNumber");
            var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "ReceiptPhoto", "receipt.jpg");

            var response = await _client.PostAsync("/api/billing/deposit-requests", content);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            var result = await response.Content.ReadFromJsonAsync<DepositRequestDto>(options);
            result.Should().NotBeNull();
            result!.Amount.Should().Be(500);
            result.Status.Should().Be(DepositStatus.Pending);
        }

        [Fact]
        public async Task SubmitDeposit_MissingPhoto_Returns400()
        {
            await PrepareDatabaseAsync();
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("500"), "Amount");
            content.Add(new StringContent("REC123"), "ReceiptNumber");

            var response = await _client.PostAsync("/api/billing/deposit-requests", content);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var problem = await response.Content.ReadAsStringAsync();
            problem.Should().Contain("Receipt photo");
        }

        [Fact]
        public async Task ReviewDeposit_Approved_CreditsBalance()
        {
            await PrepareDatabaseAsync();
            
            // First create a deposit
            string depositId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dep = new DepositRequest { ClientId = "test-user-id", Amount = 500, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Pending };
                db.DepositRequests.Add(dep);
                await db.SaveChangesAsync();
                depositId = dep.Id;
            }

            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Admin");
            var reviewDto = new { Status = DepositStatus.Approved, AdminNote = "Approved!" };

            var response = await _client.PatchAsJsonAsync($"/api/admin/billing/deposit-requests/{depositId}/review", reviewDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);



            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var wallet = await db.WalletBalances.FirstAsync(w => w.UserId == "test-user-id");
                wallet.BalanceEGP.Should().Be(1500); // 1000 seeded + 500 deposit
            }
        }

        [Fact]
        public async Task ReviewDeposit_Rejected_UnchangedBalance()
        {
            await PrepareDatabaseAsync();
            
            string depositId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dep = new DepositRequest { ClientId = "test-user-id", Amount = 500, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Pending };
                db.DepositRequests.Add(dep);
                await db.SaveChangesAsync();
                depositId = dep.Id;
            }

            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Admin");
            var reviewDto = new { Status = DepositStatus.Rejected, AdminNote = "Bad receipt" };

            var response = await _client.PatchAsJsonAsync($"/api/admin/billing/deposit-requests/{depositId}/review", reviewDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var wallet = await db.WalletBalances.FirstAsync(w => w.UserId == "test-user-id");
                wallet.BalanceEGP.Should().Be(1000); 
            }
        }

        [Fact]
        public async Task AdminEndpoint_NonAdmin_Returns403()
        {
            await PrepareDatabaseAsync();
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");

            var response = await _client.GetAsync("/api/admin/billing/deposit-requests/pending");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ReviewAlreadyReviewed_Returns422()
        {
            await PrepareDatabaseAsync();
            
            string depositId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dep = new DepositRequest { ClientId = "test-user-id", Amount = 500, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", Status = DepositStatus.Approved };
                db.DepositRequests.Add(dep);
                await db.SaveChangesAsync();
                depositId = dep.Id;
            }

            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Admin");
            var reviewDto = new { Status = DepositStatus.Approved, AdminNote = "Duplicate" };

            var response = await _client.PatchAsJsonAsync($"/api/admin/billing/deposit-requests/{depositId}/review", reviewDto);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task GetWalletBalance_ReturnsCorrectBalance()
        {
            await PrepareDatabaseAsync();
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Client");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id");

            var response = await _client.GetAsync("/api/billing/wallet-balance");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<WalletBalanceDto>();
            result!.BalanceEGP.Should().Be(1000);
        }

        [Fact]
        public async Task SubmitWithdrawal_InsufficientBalance_Returns400()
        {
            await PrepareDatabaseAsync();
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", "Freelancer");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id");

            var command = new
            {
                Amount = 2000,
                Method = WithdrawalMethod.InstaPay,
                InstapayUsername = "user1"
            };

            var response = await _client.PostAsJsonAsync("/api/billing/withdrawal-requests", command);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var problem = await response.Content.ReadAsStringAsync();
            problem.Should().Contain("Insufficient wallet balance");
        }
    }
}
