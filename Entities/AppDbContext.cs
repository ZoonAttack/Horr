using Entities.Communication;
using Entities.Marketplace;
using Entities.Payment;
using Entities.Project;
using Entities.Review;
using Entities.Skill;
using Entities.Token;
using Entities.Common;
using Entities.Users;
using Entities.Users.FreelancerHelpers;
using Entities.Verification;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Needed for the OnModelCreating loop

namespace Entities
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        // User and Profile DbSets

        public DbSet<VerificationRequest> VerificationRequests { get; set; }
        public DbSet<Specialist> SpecialistProfiles { get; set; }
        public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SavedFreelancer> SavedFreelancers { get; set; }

        // --- NEW FREELANCER PROFILE COLLECTIONS DbSets ---
        public DbSet<FreelancerLanguage> FreelancerLanguages { get; set; }
        public DbSet<FreelancerEducation> FreelancerEducation { get; set; }
        public DbSet<FreelancerExperienceDetail> FreelancerExperienceDetails { get; set; }
        public DbSet<FreelancerEmployment> FreelancerEmploymentHistory { get; set; }
        // ----------------------------------------------------

        // Skills DbSets
        public DbSet<Skill.Skill> Skills { get; set; }
        public DbSet<FreelancerSkill> FreelancerSkills { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<PortfolioMedia> PortfolioMedia { get; set; }

        // Project, Proposal, and Service DbSets
        public DbSet<ClientProject> ClientProjects { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<ServiceCatalogItem> ServiceCatalogItems { get; set; }
        public DbSet<ServicePricing> ServicePricings { get; set; }
        public DbSet<ServiceGalleryFile> ServiceGalleryFiles { get; set; }
        public DbSet<ServiceRequirement> ServiceRequirements { get; set; }
        public DbSet<ServiceStep> ServiceSteps { get; set; }
        public DbSet<ServiceFaq> ServiceFaqs { get; set; }
        public DbSet<ServiceAttribute> ServiceAttributes { get; set; }

        // Order, Chat, and Delivery DbSets
        public DbSet<Order> Orders { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }

        // Payment, Wallet, and Transaction DbSets
        public DbSet<Payment.Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<DepositRequest> DepositRequests { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        // Review and Contract DbSets
        public DbSet<Review.Review> Reviews { get; set; }
        public DbSet<SpecialistReviewRequest> SpecialistReviewRequests { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractReview> ContractReviews { get; set; }
        public DbSet<WorkDelivery> WorkDeliveries { get; set; }
        public DbSet<DeliveryAttachment> DeliveryAttachments { get; set; }
        public DbSet<ContractMilestone> ContractMilestones { get; set; }
        public DbSet<ContractDelivery> ContractDeliveries { get; set; }
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; }
        public DbSet<RevisionRequest> RevisionRequests { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<ContractSpecialistReview> ContractSpecialistReviews { get; set; }

        public DbSet<AdditionalRevisionRequest> AdditionalRevisionRequests { get; set; }

        // Job Management DbSets
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<JobMilestone> JobMilestones { get; set; }
        public DbSet<ProposalTerm> ProposalTerms { get; set; }
        public DbSet<JobInvitation> JobInvitations { get; set; }
        public DbSet<Category> Categories { get; set; }


        public DbSet<Interactions> Interactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------------------------------------------------
            // 0. GLOBAL QUERY FILTERS
            // ---------------------------------------------------------
            modelBuilder.Entity<JobPost>().HasQueryFilter(j => !j.IsDeleted);
            modelBuilder.Entity<Proposal>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Contract>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Chat>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Conversation>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Message>().HasQueryFilter(m => !m.IsDeleted && !m.Chat.IsDeleted);
            modelBuilder.Entity<ServiceCatalogItem>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<DepositRequest>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<WithdrawalRequest>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<ContractDelivery>().HasQueryFilter(cd => !cd.IsDeleted);
            modelBuilder.Entity<DeliveryAttachment>().HasQueryFilter(da => !da.IsDeleted);
            modelBuilder.Entity<ContractMilestone>().HasQueryFilter(cm => !cm.IsDeleted);
            modelBuilder.Entity<EscrowTransaction>().HasQueryFilter(et => !et.IsDeleted);
            modelBuilder.Entity<RevisionRequest>().HasQueryFilter(rr => !rr.IsDeleted);
            modelBuilder.Entity<AdditionalRevisionRequest>().HasQueryFilter(arr => !arr.IsDeleted);
            modelBuilder.Entity<Dispute>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<ContractSpecialistReview>().HasQueryFilter(r => !r.IsDeleted);


            // ---------------------------------------------------------
            // 1. MANUAL CONFIGURATION (Composite Keys & Constraints)
            // ---------------------------------------------------------

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Support both SQL Server and SQLite for default values
            var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
            var nowSql = isSqlite ? "CURRENT_TIMESTAMP" : "GETDATE()";

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql(nowSql);

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql(nowSql);


            modelBuilder.Entity<ServiceCatalogItem>(entity =>
            {
                entity.Property(e => e.CreatedAt).ValueGeneratedNever();
                entity.Property(e => e.UpdatedAt).ValueGeneratedNever();
            });

            modelBuilder.Entity<ServicePricing>()
                .HasOne(p => p.Service)
                .WithOne(s => s.Pricing)
                .HasForeignKey<ServicePricing>(p => p.ServiceId);

            // Composite Keys
            modelBuilder.Entity<FreelancerSkill>()
                .HasKey(fs => new { fs.FreelancerId, fs.SkillId });

            modelBuilder.Entity<SavedJob>()
                .HasKey(sj => new { sj.FreelancerId, sj.JobPostId });

            modelBuilder.Entity<SavedFreelancer>()
                .HasKey(sf => new { sf.ClientId, sf.FreelancerId });

            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new { js.JobPostId, js.SkillId });

            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ConversationId, cp.UserId });

            modelBuilder.Entity<Skill.Skill>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(nowSql);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql(nowSql);
                if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                    entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                }
            });

            modelBuilder.Entity<FreelancerSkill>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(nowSql);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql(nowSql);
                if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                    entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                }
            });

            modelBuilder.Entity<PortfolioItem>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(nowSql);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql(nowSql);
                if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                    entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                }
            });

            modelBuilder.Entity<PortfolioMedia>(entity =>
            {
                entity.Property(e => e.UploadedAt).HasDefaultValueSql(nowSql);
                if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
                }
            });

            modelBuilder.Entity<Proposal>()
                .HasIndex(p => new { p.FreelancerId, p.JobPostId });
                // Not unique — a freelancer can have multiple proposals per job
                // (e.g. a previous one was rejected or withdrawn).

            modelBuilder.Entity<JobInvitation>()
                .HasIndex(i => new { i.JobPostId, i.FreelancerId })
                .IsUnique();

            // Ensure SavedJob/JobPost relationship doesn't cause delete path issues if needed,
            // but fixed via bottom loop anyway.

            // Complex Relationships
            modelBuilder.Entity<ClientProject>()
                .HasOne(p => p.AcceptedProposal)
                .WithOne()
                .HasForeignKey<ClientProject>(p => p.AcceptedProposalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Proposal)
                .WithOne()
                .HasForeignKey<Contract>(c => c.ProposalId);

            modelBuilder.Entity<ContractReview>()
                .HasIndex(r => new { r.ContractId, r.ReviewerId })
                .IsUnique();

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

            modelBuilder.Entity<WalletBalance>()
                .HasIndex(wb => wb.UserId)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their tokens

            // ---------------------------------------------------------
            // 3. THE GLOBAL FIX (Must be at the Bottom)
            // ---------------------------------------------------------

            // This loop finds EVERY relationship in your database (Orders, Chats, Deliveries, etc.)
            // and changes the delete behavior to 'Restrict'.
            // This effectively stops the "Multiple Cascade Paths" error for the whole project.
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            SeedSkills(modelBuilder);
        }

        private static void SeedSkills(ModelBuilder modelBuilder)
        {
            const string techId = "5f8e6b3a-9c2d-4b7a-8f1e-3d2c1b0a9f8e";
            const string designId = "7d9c8b7a-6f5e-4d3c-b2a1-0f9e8d7c6b5a";
            const string dataAiId = "2a1b0c9d-8e7f-6a5b-4c3d-2e1f0a9b8c7d";
            const string writingMarketingId = "4e3d2c1b-0a9f-8e7d-6c5b-4a3b2c1d0e9f";
            const string otherId = "6c5b4a3b-2c1d-0e9f-8d7c-6b5a4f3e2d1c";

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = techId, Name = "Technology", Slug = "technology" },
                new Category { Id = designId, Name = "Design", Slug = "design" },
                new Category { Id = dataAiId, Name = "Data & AI", Slug = "data-ai" },
                new Category { Id = writingMarketingId, Name = "Writing & Marketing", Slug = "writing-marketing" },
                new Category { Id = otherId, Name = "Other", Slug = "other" }
            );

            modelBuilder.Entity<Skill.Skill>().HasData(
                // Technology
                new Skill.Skill { Id = "skill-csharp", Name = "C#", CategoryId = techId },
                new Skill.Skill { Id = "skill-javascript", Name = "JavaScript", CategoryId = techId },
                new Skill.Skill { Id = "skill-typescript", Name = "TypeScript", CategoryId = techId },
                new Skill.Skill { Id = "skill-python", Name = "Python", CategoryId = techId },
                new Skill.Skill { Id = "skill-java", Name = "Java", CategoryId = techId },
                new Skill.Skill { Id = "skill-php", Name = "PHP", CategoryId = techId },
                new Skill.Skill { Id = "skill-swift", Name = "Swift", CategoryId = techId },
                new Skill.Skill { Id = "skill-kotlin", Name = "Kotlin", CategoryId = techId },
                new Skill.Skill { Id = "skill-go", Name = "Go", CategoryId = techId },
                new Skill.Skill { Id = "skill-rust", Name = "Rust", CategoryId = techId },
                new Skill.Skill { Id = "skill-react", Name = "React", CategoryId = techId },
                new Skill.Skill { Id = "skill-angular", Name = "Angular", CategoryId = techId },
                new Skill.Skill { Id = "skill-vuejs", Name = "Vue.js", CategoryId = techId },
                new Skill.Skill { Id = "skill-nodejs", Name = "Node.js", CategoryId = techId },
                new Skill.Skill { Id = "skill-aspnetcore", Name = "ASP.NET Core", CategoryId = techId },
                new Skill.Skill { Id = "skill-django", Name = "Django", CategoryId = techId },
                new Skill.Skill { Id = "skill-laravel", Name = "Laravel", CategoryId = techId },
                new Skill.Skill { Id = "skill-springboot", Name = "Spring Boot", CategoryId = techId },
                new Skill.Skill { Id = "skill-flutter", Name = "Flutter", CategoryId = techId },
                new Skill.Skill { Id = "skill-reactnative", Name = "React Native", CategoryId = techId },

                // Design
                new Skill.Skill { Id = "skill-uidesign", Name = "UI Design", CategoryId = designId },
                new Skill.Skill { Id = "skill-uxdesign", Name = "UX Design", CategoryId = designId },
                new Skill.Skill { Id = "skill-graphicdesign", Name = "Graphic Design", CategoryId = designId },
                new Skill.Skill { Id = "skill-logodesign", Name = "Logo Design", CategoryId = designId },
                new Skill.Skill { Id = "skill-figma", Name = "Figma", CategoryId = designId },
                new Skill.Skill { Id = "skill-adobexd", Name = "Adobe XD", CategoryId = designId },
                new Skill.Skill { Id = "skill-photoshop", Name = "Photoshop", CategoryId = designId },
                new Skill.Skill { Id = "skill-illustrator", Name = "Illustrator", CategoryId = designId },
                new Skill.Skill { Id = "skill-motiongraphics", Name = "Motion Graphics", CategoryId = designId },
                new Skill.Skill { Id = "skill-videoediting", Name = "Video Editing", CategoryId = designId },

                // Data & AI
                new Skill.Skill { Id = "skill-machinelearning", Name = "Machine Learning", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-dataanalysis", Name = "Data Analysis", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-sql", Name = "SQL", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-mongodb", Name = "MongoDB", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-postgresql", Name = "PostgreSQL", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-powerbi", Name = "Power BI", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-tableau", Name = "Tableau", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-tensorflow", Name = "TensorFlow", CategoryId = dataAiId },
                new Skill.Skill { Id = "skill-datascience", Name = "Data Science", CategoryId = dataAiId },

                // Writing & Marketing
                new Skill.Skill { Id = "skill-contentwriting", Name = "Content Writing", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-copywriting", Name = "Copywriting", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-seo", Name = "SEO", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-socialmediamarketing", Name = "Social Media Marketing", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-emailmarketing", Name = "Email Marketing", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-technicalwriting", Name = "Technical Writing", CategoryId = writingMarketingId },
                new Skill.Skill { Id = "skill-translation", Name = "Translation", CategoryId = writingMarketingId },

                // Other
                new Skill.Skill { Id = "skill-projectmanagement", Name = "Project Management", CategoryId = otherId },
                new Skill.Skill { Id = "skill-devops", Name = "DevOps", CategoryId = otherId },
                new Skill.Skill { Id = "skill-docker", Name = "Docker", CategoryId = otherId },
                new Skill.Skill { Id = "skill-kubernetes", Name = "Kubernetes", CategoryId = otherId },
                new Skill.Skill { Id = "skill-cybersecurity", Name = "Cybersecurity", CategoryId = otherId },
                new Skill.Skill { Id = "skill-aws", Name = "AWS", CategoryId = otherId },
                new Skill.Skill { Id = "skill-azure", Name = "Azure", CategoryId = otherId },
                new Skill.Skill { Id = "skill-networking", Name = "Networking", CategoryId = otherId }
            );
        }
    }
}
