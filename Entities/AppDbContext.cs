using Entities.Communication;
using Entities.Marketplace;
using Entities.Payment;
using Entities.Project;
using Entities.Review;
using Entities.Skill;
using Entities.Token;
using Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        // User and Profile DbSets
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<Specialist> SpecialistProfiles { get; set; }
        public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Client> Clients { get; set; }

        // Skills DbSets
        public DbSet<Skill.Skill> Skills { get; set; }
        public DbSet<FreelancerSkill> FreelancerSkills { get; set; }

        // Project, Proposal, and Service DbSets
        public DbSet<ClientProject> ClientProjects { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Service> Services { get; set; }

        // Order, Chat, and Delivery DbSets
        public DbSet<Order> Orders { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }

        // Payment, Wallet, and Transaction DbSets
        public DbSet<Payment.Payment> Payments { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        // Review and Contract DbSets
        public DbSet<Review.Review> Reviews { get; set; }
        public DbSet<SpecialistReviewRequest> SpecialistReviewRequests { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------------------------------------------------
            // 1. MANUAL CONFIGURATION (Composite Keys & Constraints)
            // ---------------------------------------------------------

            // Composite Keys
            modelBuilder.Entity<FreelancerSkill>()
                .HasKey(fs => new { fs.FreelancerId, fs.SkillId });

            // Complex Relationships
            modelBuilder.Entity<ClientProject>()
                .HasOne(p => p.AcceptedProposal)
                .WithOne()
                .HasForeignKey<ClientProject>(p => p.AcceptedProposalId)
                .OnDelete(DeleteBehavior.Restrict);

            // CHECK Constraints (PascalCase Fixed)
            modelBuilder.Entity<Order>()
                .ToTable(t => t.HasCheckConstraint("CHK_orders_type_relation",
                    "([OrderType] = 0 AND [ServiceId] IS NOT NULL AND [ProjectId] IS NULL) OR " +
                    "([OrderType] = 1 AND [ProjectId] IS NOT NULL AND [ServiceId] IS NULL)"));

            modelBuilder.Entity<Review.Review>()
                .ToTable(t => t.HasCheckConstraint("CHK_reviews_diff_users",
                    "[ReviewerId] <> [RevieweeId]"));

            modelBuilder.Entity<Review.Review>()
                .ToTable(t => t.HasCheckConstraint("CHK_reviews_project_or_order",
                    "([ProjectId] IS NOT NULL AND [OrderId] IS NULL) OR " +
                    "([ProjectId] IS NULL AND [OrderId] IS NOT NULL)"));

            modelBuilder.Entity<Transaction>()
                .ToTable(t => t.HasCheckConstraint("CHK_transactions_wallets",
                    "([TransactionType] = 0 AND [ReceiverWalletId] IS NOT NULL AND [SenderWalletId] IS NULL) OR " +
                    "([TransactionType] = 1 AND [SenderWalletId] IS NOT NULL AND [ReceiverWalletId] IS NULL) OR " +
                    "([TransactionType] IN (2, 3, 4, 5) AND [SenderWalletId] IS NOT NULL AND [ReceiverWalletId] IS NOT NULL)"));
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their tokens

            // ---------------------------------------------------------
            // 2. THE GLOBAL FIX (Must be at the Bottom)
            // ---------------------------------------------------------

            // This loop finds EVERY relationship in your database (Orders, Chats, Deliveries, etc.)
            // and changes the delete behavior to 'Restrict'.
            // This effectively stops the "Multiple Cascade Paths" error for the whole project.
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
