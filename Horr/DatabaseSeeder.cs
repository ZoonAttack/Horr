using Entities;
using Entities.Enums;
using Entities.Payment;
using Entities.Project;
using Entities.Skill;
using Entities.Users;
using Entities.Review;
using Entities.Communication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Horr
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply migrations automatically (this will also apply the HasData seeds in AppDbContext)
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Client", "Freelancer", "Specialist" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@test.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    EmailConfirmed = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await EnsureWalletAsync(context, adminUser.Id);
                }
            }

            // 3. Seed Clients (Omar, Sarah, David)
            var clientEmails = new[] { "client1@test.com", "client2@test.com", "client3@test.com" };
            var clientNames = new[] { "Omar Alsawah", "Sarah Tech", "David Product" };
            var clientBios = new[] { "Product Manager at Vampires Studio", "Tech Lead at Creative Solutions", "Director at Agile Corp" };
            var clientUsers = new User[3];

            for (int i = 0; i < 3; i++)
            {
                var email = clientEmails[i];
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = clientNames[i],
                        Role = UserRole.Client,
                        EmailConfirmed = true,
                        IsVerified = true,
                        Bio = clientBios[i],
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Client");
                        var profile = new Client
                        {
                            UserId = user.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Clients.Add(profile);
                        await context.SaveChangesAsync();
                        await EnsureWalletAsync(context, user.Id, 50000);
                    }
                }
                clientUsers[i] = user;
            }

            // 4. Seed Freelancers (Specialist Dev, Alice, Bob)
            var freelancerEmails = new[] { "freelancer1@test.com", "freelancer2@test.com", "freelancer3@test.com" };
            var freelancerNames = new[] { "Specialist Dev", "Alice Csharp", "Bob Writer" };
            var freelancerTitles = new[] { "Senior Full-Stack Engineer", "Senior Backend Developer", "Technical Copywriter" };
            var freelancerRates = new decimal?[] { 50, 60, 30 };
            var freelancerUsers = new User[3];

            for (int i = 0; i < 3; i++)
            {
                var email = freelancerEmails[i];
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = freelancerNames[i],
                        Role = UserRole.Freelancer,
                        EmailConfirmed = true,
                        IsVerified = true,
                        Bio = $"Professional {freelancerTitles[i]} with years of industry expertise.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Freelancer");
                        var profile = new Freelancer
                        {
                            UserId = user.Id,
                            Title = freelancerTitles[i],
                            HourlyRate = freelancerRates[i],
                            Availability = "Full-time",
                            YearsOfExperience = 5 + i,
                            ExperienceLevel = ExperienceLevel.Expert,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Freelancers.Add(profile);
                        await context.SaveChangesAsync();
                        await EnsureWalletAsync(context, user.Id, 10000);
                    }
                }
                freelancerUsers[i] = user;
            }

            // 5. Seed Specialist
            var specialistEmail = "specialist1@test.com";
            var specialistUser = await userManager.FindByEmailAsync(specialistEmail);
            if (specialistUser == null)
            {
                specialistUser = new User
                {
                    UserName = specialistEmail,
                    Email = specialistEmail,
                    FullName = "Specialist Mark",
                    Role = UserRole.Specialist,
                    EmailConfirmed = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(specialistUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(specialistUser, "Specialist");
                    var profile = new Specialist
                    {
                        UserId = specialistUser.Id,
                        Specialization = "Senior Platform Review Architect",
                        ProfilePicturePath = "",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.SpecialistProfiles.Add(profile);
                    await context.SaveChangesAsync();
                    await EnsureWalletAsync(context, specialistUser.Id);
                }
            }

            // 6. Get Categories & Skills mapping
            var category = await context.Categories.FirstOrDefaultAsync();
            if (category == null)
            {
                return; // SeedSkills in AppDbContext handles categories
            }

            // 7. Seed Job Posts (4 posts)
            var jobsToSeed = new[]
            {
                new { Title = "Responsive Portfolio Website Refactoring", Budget = 8500m, ClientIdx = 0, Description = "Clean refactoring of a personal profile website. Modern components, Tailwind CSS styling, responsive layout optimization, and multi-language support integration." },
                new { Title = "ASP.NET Core E-Commerce API Development", Budget = 15000m, ClientIdx = 1, Description = "Build a secure e-commerce RESTful API using ASP.NET Core 8, EF Core, and integrate Instapay or equivalent mock gateway." },
                new { Title = "UI/UX Mobile App Redesign", Budget = 5000m, ClientIdx = 0, Description = "Redesign mobile app screens using Figma. Focusing on user experience enhancements, high fidelity assets export, and theme setups." },
                new { Title = "Technical Blog Writing for Devs", Budget = 3000m, ClientIdx = 2, Description = "Write 5 high-quality technical blog articles about frontend performance optimization and C# .NET Web API best practices." }
            };

            var jobPosts = new List<JobPost>();
            foreach (var job in jobsToSeed)
            {
                var post = await context.JobPosts.FirstOrDefaultAsync(j => j.Title == job.Title);
                if (post == null && clientUsers[job.ClientIdx] != null)
                {
                    post = new JobPost
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = job.Title,
                        Description = job.Description,
                        CategoryId = category.Id,
                        Budget = job.Budget,
                        BudgetCurrency = "USD",
                        JobType = JobType.FixedPrice,
                        Scope = ProjectComplexity.Medium,
                        ExperienceLevel = ExperienceLevel.Intermediate,
                        ClientId = clientUsers[job.ClientIdx].Id,
                        PostedAt = DateTime.UtcNow.AddDays(-10),
                        IsDeleted = false
                    };
                    context.JobPosts.Add(post);

                    // Map some skills
                    var skills = await context.Skills.Take(3).ToListAsync();
                    foreach (var skill in skills)
                    {
                        context.JobSkills.Add(new JobSkill { JobPostId = post.Id, SkillId = skill.Id });
                    }
                    await context.SaveChangesAsync();
                }
                if (post != null)
                {
                    jobPosts.Add(post);
                }
            }

            // 8. Seed Proposals (7 Proposals)
            var proposalsToSeed = new[]
            {
                // Job 1
                new { JobIdx = 0, FreelancerIdx = 0, Bid = 8500m, Status = ProposalStatus.Offer, Cover = "I have extensive experience refactoring portfolio sites and integrating design tokens." },
                new { JobIdx = 0, FreelancerIdx = 1, Bid = 9000m, Status = ProposalStatus.Rejected, Cover = "I can do this portfolio refactoring within 3 days." },
                // Job 2
                new { JobIdx = 1, FreelancerIdx = 1, Bid = 15000m, Status = ProposalStatus.Offer, Cover = "Senior backend engineer here. I can write clean e-commerce APIs with full documentation." },
                new { JobIdx = 1, FreelancerIdx = 2, Bid = 14000m, Status = ProposalStatus.Submitted, Cover = "I have built similar .NET Web APIs before. I bid 14k." },
                // Job 3
                new { JobIdx = 2, FreelancerIdx = 0, Bid = 5000m, Status = ProposalStatus.Submitted, Cover = "Figma expert ready to redesign your mobile screens." },
                new { JobIdx = 2, FreelancerIdx = 2, Bid = 4500m, Status = ProposalStatus.Withdrawn, Cover = "I withdraw my bid due to other project engagements." },
                // Job 4
                new { JobIdx = 3, FreelancerIdx = 2, Bid = 3000m, Status = ProposalStatus.Offer, Cover = "I write clear technical articles with C# code samples." }
            };

            var proposals = new List<Proposal>();
            foreach (var prop in proposalsToSeed)
            {
                if (jobPosts.Count > prop.JobIdx && freelancerUsers[prop.FreelancerIdx] != null)
                {
                    var jobPost = jobPosts[prop.JobIdx];
                    var freelancer = freelancerUsers[prop.FreelancerIdx];

                    var proposal = await context.Proposals.FirstOrDefaultAsync(p => p.JobPostId == jobPost.Id && p.FreelancerId == freelancer.Id);
                    if (proposal == null)
                    {
                        proposal = new Proposal
                        {
                            JobPostId = jobPost.Id,
                            FreelancerId = freelancer.Id,
                            BidAmount = prop.Bid,
                            BidCurrency = "USD",
                            BidRate = prop.Bid,
                            HORRFee = prop.Bid * 0.10m,
                            CoverLetter = prop.Cover,
                            Status = prop.Status,
                            DurationDays = 30,
                            MaxRevisions = 3,
                            CreatedAt = DateTime.UtcNow.AddDays(-8),
                            IsDeleted = false
                        };
                        context.Proposals.Add(proposal);
                        await context.SaveChangesAsync();
                    }
                    proposals.Add(proposal);
                }
            }

            // 9. Seed Contracts (3 Contracts - No Milestones seeded)
            if (proposals.Count >= 7)
            {
                // Contract 1: Active, Freelancer 1 + Client 1, Budget 8500
                var contract1 = await context.Contracts.FirstOrDefaultAsync(c => c.ProposalId == proposals[0].Id);
                if (contract1 == null)
                {
                    contract1 = new Contract
                    {
                        ProposalId = proposals[0].Id,
                        JobPostId = jobPosts[0].Id,
                        ClientId = clientUsers[0].Id,
                        FreelancerId = freelancerUsers[0].Id,
                        AgreedRate = 8500,
                        TotalAmount = 8500,
                        OriginalCurrency = "USD",
                        LockedExchangeRate = 1.0m,
                        DurationDays = 30,
                        DueDate = DateTime.UtcNow.AddDays(23),
                        MaxRevisions = 3,
                        CustomJobDescription = jobPosts[0].Description,
                        Status = ContractStatus.Active,
                        StartedAt = DateTime.UtcNow.AddDays(-7),
                        AcceptedAt = DateTime.UtcNow.AddDays(-7),
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        IsDeleted = false
                    };
                    context.Contracts.Add(contract1);
                    await context.SaveChangesAsync();

                    // Seed ContractDelivery 1 (Active, work submitted -> Under Review)
                    var delivery1 = new ContractDelivery
                    {
                        Id = Guid.NewGuid(),
                        ContractId = contract1.Id,
                        SubmittedAt = DateTime.UtcNow.AddDays(-1),
                        DeliveryNote = "Refactored portfolio home page. Staging URL attached.",
                        Status = DeliveryStatus.Pending,
                        ReviewDeadline = DateTime.UtcNow.AddDays(2),
                        IsDeleted = false
                    };
                    context.ContractDeliveries.Add(delivery1);
                    await context.SaveChangesAsync();

                    var attach1 = new DeliveryAttachment
                    {
                        Id = Guid.NewGuid(),
                        DeliveryId = delivery1.Id,
                        Type = AttachmentType.Link,
                        FileName = "staging_environment",
                        FileType = "Link",
                        FileSizeBytes = 0,
                        Url = "https://staging-v2.horr-freelance.app",
                        FileUrl = "https://staging-v2.horr-freelance.app",
                        OriginalFileName = "staging_environment",
                        UploadedAt = DateTime.UtcNow.AddDays(-1)
                    };
                    context.DeliveryAttachments.Add(attach1);
                    await context.SaveChangesAsync();
                }

                // Contract 2: Active, Freelancer 2 + Client 2, Budget 15000 (No deliveries -> Submit Work)
                var contract2 = await context.Contracts.FirstOrDefaultAsync(c => c.ProposalId == proposals[2].Id);
                if (contract2 == null)
                {
                    contract2 = new Contract
                    {
                        ProposalId = proposals[2].Id,
                        JobPostId = jobPosts[1].Id,
                        ClientId = clientUsers[1].Id,
                        FreelancerId = freelancerUsers[1].Id,
                        AgreedRate = 15000,
                        TotalAmount = 15000,
                        OriginalCurrency = "USD",
                        LockedExchangeRate = 1.0m,
                        DurationDays = 30,
                        DueDate = DateTime.UtcNow.AddDays(24),
                        MaxRevisions = 3,
                        CustomJobDescription = jobPosts[1].Description,
                        Status = ContractStatus.Active,
                        StartedAt = DateTime.UtcNow.AddDays(-6),
                        AcceptedAt = DateTime.UtcNow.AddDays(-6),
                        CreatedAt = DateTime.UtcNow.AddDays(-6),
                        IsDeleted = false
                    };
                    context.Contracts.Add(contract2);
                    await context.SaveChangesAsync();
                }

                // Contract 3: Completed, Freelancer 3 + Client 3, Budget 3000
                var contract3 = await context.Contracts.FirstOrDefaultAsync(c => c.ProposalId == proposals[6].Id);
                if (contract3 == null)
                {
                    contract3 = new Contract
                    {
                        ProposalId = proposals[6].Id,
                        JobPostId = jobPosts[3].Id,
                        ClientId = clientUsers[2].Id,
                        FreelancerId = freelancerUsers[2].Id,
                        AgreedRate = 3000,
                        TotalAmount = 3000,
                        OriginalCurrency = "USD",
                        LockedExchangeRate = 1.0m,
                        DurationDays = 6,
                        DueDate = DateTime.UtcNow.AddDays(-3),
                        MaxRevisions = 3,
                        CustomJobDescription = jobPosts[3].Description,
                        Status = ContractStatus.Completed,
                        StartedAt = DateTime.UtcNow.AddDays(-9),
                        AcceptedAt = DateTime.UtcNow.AddDays(-9),
                        ClosedAt = DateTime.UtcNow.AddDays(-3),
                        CreatedAt = DateTime.UtcNow.AddDays(-9),
                        IsDeleted = false
                    };
                    context.Contracts.Add(contract3);
                    await context.SaveChangesAsync();

                    // Seed Approved Delivery
                    var delivery3 = new ContractDelivery
                    {
                        Id = Guid.NewGuid(),
                        ContractId = contract3.Id,
                        SubmittedAt = DateTime.UtcNow.AddDays(-4),
                        DeliveryNote = "Draft and final version of 5 technical articles.",
                        Status = DeliveryStatus.Approved,
                        ReviewDeadline = DateTime.UtcNow.AddDays(-1),
                        CompletedAt = DateTime.UtcNow.AddDays(-3),
                        IsDeleted = false
                    };
                    context.ContractDeliveries.Add(delivery3);
                    await context.SaveChangesAsync();

                    var attach3 = new DeliveryAttachment
                    {
                        Id = Guid.NewGuid(),
                        DeliveryId = delivery3.Id,
                        Type = AttachmentType.File,
                        FileName = "5_technical_articles.zip",
                        FileType = "application/zip",
                        StoragePath = "uploads/5_technical_articles.zip",
                        FileUrl = "uploads/5_technical_articles.zip",
                        OriginalFileName = "5_technical_articles.zip",
                        FileSizeBytes = 4096000,
                        UploadedAt = DateTime.UtcNow.AddDays(-4)
                    };
                    context.DeliveryAttachments.Add(attach3);
                    await context.SaveChangesAsync();

                    // Seed ContractReviews (Client to Freelancer & Freelancer to Client)
                    var reviewClient = new ContractReview
                    {
                        ContractId = contract3.Id,
                        ReviewerId = clientUsers[2].Id,
                        Rating = 5,
                        Comment = "Excellent writing skills! Highly recommended.",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    };
                    var reviewFreelancer = new ContractReview
                    {
                        ContractId = contract3.Id,
                        ReviewerId = freelancerUsers[2].Id,
                        Rating = 5,
                        Comment = "Great communication, clear requirements.",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    };
                    context.ContractReviews.AddRange(reviewClient, reviewFreelancer);
                    await context.SaveChangesAsync();
                }

                // Seed Chats and Messages
                if (contract1 != null)
                {
                    var chat1 = await context.Chats.FirstOrDefaultAsync(ch => ch.ContractId == contract1.Id);
                    if (chat1 == null)
                    {
                        chat1 = new Chat
                        {
                            Id = Guid.NewGuid().ToString(),
                            ContractId = contract1.Id,
                            ClientId = clientUsers[0].Id,
                            FreelancerId = freelancerUsers[0].Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-7),
                            IsDeleted = false
                        };
                        context.Chats.Add(chat1);
                        await context.SaveChangesAsync();

                        var messages1 = new List<Message>
                        {
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat1.Id,
                                SenderId = clientUsers[0].Id,
                                Body = "Hi there! Welcome to the project. Let's start with refactoring the home page.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-7),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat1.Id,
                                SenderId = freelancerUsers[0].Id,
                                Body = "Hi Omar! Thanks for the offer. I've set up the repository and the initial design tokens.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-7).AddHours(1),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat1.Id,
                                SenderId = freelancerUsers[0].Id,
                                Body = "I've also deployed a staging environment. Let me know if you have any feedback on the styling.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat1.Id,
                                SenderId = clientUsers[0].Id,
                                Body = "Looks great! I'll review the staging link and approve the submission.",
                                Status = MessageStatus.Unread,
                                SentAt = DateTime.UtcNow.AddHours(-1),
                                Type = MessageType.Text,
                                IsDeleted = false
                            }
                        };
                        context.Messages.AddRange(messages1);
                        await context.SaveChangesAsync();
                    }
                }

                if (contract2 != null)
                {
                    var chat2 = await context.Chats.FirstOrDefaultAsync(ch => ch.ContractId == contract2.Id);
                    if (chat2 == null)
                    {
                        chat2 = new Chat
                        {
                            Id = Guid.NewGuid().ToString(),
                            ContractId = contract2.Id,
                            ClientId = clientUsers[1].Id,
                            FreelancerId = freelancerUsers[1].Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-6),
                            IsDeleted = false
                        };
                        context.Chats.Add(chat2);
                        await context.SaveChangesAsync();

                        var messages2 = new List<Message>
                        {
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat2.Id,
                                SenderId = clientUsers[1].Id,
                                Body = "Hello Alice, excited to work with you on the ASP.NET Core E-Commerce API.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-6),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat2.Id,
                                SenderId = freelancerUsers[1].Id,
                                Body = "Hi Sarah! Glad to be on board. I am outlining the database schema and API endpoints now.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-6).AddHours(2),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat2.Id,
                                SenderId = freelancerUsers[1].Id,
                                Body = "I will share the schema details tomorrow for your review before starting code implementation.",
                                Status = MessageStatus.Unread,
                                SentAt = DateTime.UtcNow.AddHours(-2),
                                Type = MessageType.Text,
                                IsDeleted = false
                            }
                        };
                        context.Messages.AddRange(messages2);
                        await context.SaveChangesAsync();
                    }
                }

                if (contract3 != null)
                {
                    var chat3 = await context.Chats.FirstOrDefaultAsync(ch => ch.ContractId == contract3.Id);
                    if (chat3 == null)
                    {
                        chat3 = new Chat
                        {
                            Id = Guid.NewGuid().ToString(),
                            ContractId = contract3.Id,
                            ClientId = clientUsers[2].Id,
                            FreelancerId = freelancerUsers[2].Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-9),
                            IsDeleted = false
                        };
                        context.Chats.Add(chat3);
                        await context.SaveChangesAsync();

                        var messages3 = new List<Message>
                        {
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat3.Id,
                                SenderId = clientUsers[2].Id,
                                Body = "Hi Bob, I need 5 technical blog posts about frontend optimization.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-9),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat3.Id,
                                SenderId = freelancerUsers[2].Id,
                                Body = "Hi David, absolutely. I will draft them and send a zip attachment soon.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-9).AddHours(1),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat3.Id,
                                SenderId = freelancerUsers[2].Id,
                                Body = "I have submitted the work delivery zip file for your review.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-4),
                                Type = MessageType.Text,
                                IsDeleted = false
                            },
                            new Message
                            {
                                Id = Guid.NewGuid().ToString(),
                                ChatId = chat3.Id,
                                SenderId = clientUsers[2].Id,
                                Body = "Excellent work! I've approved the work and completed the contract.",
                                Status = MessageStatus.Read,
                                SentAt = DateTime.UtcNow.AddDays(-3),
                                Type = MessageType.Text,
                                IsDeleted = false
                            }
                        };
                        context.Messages.AddRange(messages3);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task EnsureWalletAsync(AppDbContext context, string userId, decimal initialBalance = 0)
        {
            var wallet = await context.WalletBalances.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new WalletBalance
                {
                    UserId = userId,
                    BalanceEGP = initialBalance,
                    LastUpdatedAt = DateTime.UtcNow
                };
                context.WalletBalances.Add(wallet);
                await context.SaveChangesAsync();
            }
        }
    }
}
