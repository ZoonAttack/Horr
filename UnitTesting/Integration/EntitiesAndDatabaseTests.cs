using Entities;
using Entities.Enums;
using Entities.Payment;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTesting.Integration
{
    public class EntitiesAndDatabaseTests : IDisposable
    {
        private readonly AppDbContext _context;

        public EntitiesAndDatabaseTests()
        {
            _context = DbContextUtility.CreateSqliteDbContext(Guid.NewGuid().ToString());
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task WalletBalance_UserId_UniqueIndex_RejectsDuplicate()
        {
            // Arrange
            var user1 = new Entities.Users.User { Id = Guid.NewGuid().ToString(), FullName = "User 1", UserName = "user1", Email = "user1@test.com", Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC" };
            var user2 = new Entities.Users.User { Id = Guid.NewGuid().ToString(), FullName = "User 2", UserName = "user2", Email = "user2@test.com", Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var wallet1 = new WalletBalance { UserId = user1.Id, BalanceEGP = 100 };
            _context.WalletBalances.Add(wallet1);
            await _context.SaveChangesAsync();

            // Act & Assert
            var wallet2 = new WalletBalance { UserId = user1.Id, BalanceEGP = 200 };
            _context.WalletBalances.Add(wallet2);

            await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
        }

        [Fact]
        public async Task DepositRequest_GlobalQueryFilter_ExcludesSoftDeleted()
        {
            // Arrange
            var user = new Entities.Users.User { Id = Guid.NewGuid().ToString(), FullName = "Client", UserName = "client", Email = "client@test.com", Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request1 = new DepositRequest { ClientId = user.Id, Amount = 100, ReceiptNumber = "R1", ReceiptPhotoUrl = "P1", IsDeleted = false };
            var request2 = new DepositRequest { ClientId = user.Id, Amount = 200, ReceiptNumber = "R2", ReceiptPhotoUrl = "P2", IsDeleted = true };
            _context.DepositRequests.AddRange(request1, request2);
            await _context.SaveChangesAsync();

            // Act
            var requests = await _context.DepositRequests.ToListAsync();

            // Assert
            Assert.Single(requests);
            Assert.Equal(100, requests[0].Amount);
        }

        [Fact]
        public async Task WithdrawalRequest_GlobalQueryFilter_ExcludesSoftDeleted()
        {
            // Arrange
            var user = new Entities.Users.User { Id = Guid.NewGuid().ToString(), FullName = "Freelancer", UserName = "freelancer", Email = "free@test.com", Address = "A", City = "C", Country = "Egypt", Bio = "B", StateProvince = "S", ZipCode = "1", TimeZone = "UTC" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request1 = new WithdrawalRequest { FreelancerId = user.Id, Amount = 100, Method = WithdrawalMethod.InstaPay, Status = WithdrawalStatus.Pending, IsDeleted = false };
            var request2 = new WithdrawalRequest { FreelancerId = user.Id, Amount = 200, Method = WithdrawalMethod.InstaPay, Status = WithdrawalStatus.Pending, IsDeleted = true };
            _context.WithdrawalRequests.AddRange(request1, request2);
            await _context.SaveChangesAsync();

            // Act
            var requests = await _context.WithdrawalRequests.ToListAsync();

            // Assert
            Assert.Single(requests);
            Assert.Equal(100, requests[0].Amount);
        }
    }
}
