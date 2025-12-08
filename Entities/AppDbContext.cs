using Entities.Communication;
using Entities.Marketplace;
using Entities.Payment;
using Entities.Project;
using Entities.Review;
using Entities.Skill;
using Entities.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class AppDbContext : IdentityDbContext<User.User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}

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

            // --- Handle Composite Keys ---
            // Data Annotations [Key, Column(Order=X)] handle this, 
            // but explicit definition here is also common.
            modelBuilder.Entity<FreelancerSkill>()
                .HasKey(fs => new { fs.FreelancerId, fs.SkillId });

            // --- Handle Complex Relationships (Cycles) ---
            // The 1-to-1 relationship between ClientProject and its AcceptedProposal
            // can be tricky. We need to define the principal and dependent ends clearly.
            modelBuilder.Entity<ClientProject>()
                .HasOne(p => p.AcceptedProposal)
                .WithOne() // No inverse navigation property for this specific 1-to-1
                .HasForeignKey<ClientProject>(p => p.AcceptedProposalId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a proposal if it's accepted

            // --- Handle Multiple Relationships (e.g., User -> Review) ---
            modelBuilder.Entity<Review.Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they gave reviews

            modelBuilder.Entity<Review.Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they received reviews

            // --- Handle Multiple Relationships (e.g., Wallet -> Transaction) ---
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SenderWallet)
                .WithMany(w => w.SentTransactions)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict); // Or .SetNull

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReceiverWallet)
                .WithMany(w => w.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict); // Or .SetNull

            // --- Add CHECK Constraints not supported by Annotations ---
            // This is where you would add the rules from the [Comment] attributes.
            // Example for the 'orders' table constraint:
            // Note: Enums are stored as integers, so Service = 0, Project = 1
            modelBuilder.Entity<Order>()
                .ToTable(t => t.HasCheckConstraint("CHK_orders_type_relation",
                    "([OrderType] = 0 AND [service_id] IS NOT NULL AND [project_id] IS NULL) OR " +
                    "([OrderType] = 1 AND [project_id] IS NOT NULL AND [service_id] IS NULL)"));

            // Example for the 'reviews' table constraint:
            modelBuilder.Entity<Review.Review>()
                .ToTable(t => t.HasCheckConstraint("CHK_reviews_diff_users",
                    "[reviewer_id] <> [reviewee_id]"));

            // Ensure Review has either ProjectId or OrderId, not both or neither
            modelBuilder.Entity<Review.Review>()
                .ToTable(t => t.HasCheckConstraint("CHK_reviews_project_or_order",
                    "([project_id] IS NOT NULL AND [order_id] IS NULL) OR " +
                    "([project_id] IS NULL AND [order_id] IS NOT NULL)"));

            // Example for 'transactions' wallet check (logic depends on SQL dialect)
            // Note: TransactionType enum: Deposit=0, Withdrawal=1, Transfer=2, Refund=3, Commission=4, Escrow=5
            modelBuilder.Entity<Transaction>()
                .ToTable(t => t.HasCheckConstraint("CHK_transactions_wallets",
                    "([TransactionType] = 0 AND [receiver_wallet_id] IS NOT NULL AND [sender_wallet_id] IS NULL) OR " +
                    "([TransactionType] = 1 AND [sender_wallet_id] IS NOT NULL AND [receiver_wallet_id] IS NULL) OR " +
                    "([TransactionType] IN (2, 3, 4, 5) AND [sender_wallet_id] IS NOT NULL AND [receiver_wallet_id] IS NOT NULL)"));
        }
    }
}
