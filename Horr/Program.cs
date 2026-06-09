using Entities;
using Entities.Users;
using Horr.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts.Settings;
using ServiceImplementation.Authentication;
using ServiceImplementation.Implementations.Settings;
using Services.Authentication;
using Services.Implementations;
using System.Text;
using ServiceImplementation.Hubs;
using Services.Client;
using ServiceImplementation.Implementations.ClientImplementation;
using ServiceContracts;
using ServiceImplementation.Implementations;
using Services.Freelancer;
using ServiceImplementation.Implementations.FreelancerImplementation;
using ServiceContracts.AI;
using ServiceContracts.Recommendations;
using ServiceImplementation.Implementations.AI;
using ServiceImplementation.Implementations.Recommendations;
using ServiceContracts.Storage;
using ServiceImplementation.Storage;
using Services.Wallet;
using ServiceImplementation.Implementations.Wallet;

namespace Horr
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // 1. DATABASE & IDENTITY SETUP
            // ==========================================
            if (builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestsDb"));
            }
            else
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            }

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false; // Adjust as needed
                options.User.RequireUniqueEmail = true; // Ensure unique emails
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ==========================================
            // 2. REGISTER YOUR CUSTOM SERVICES (DI)
            // ==========================================
            // This tells ASP.NET: "When a controller asks for IAuthService, give them AuthService"
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IProfileSettings, ProfileSettings>();
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<Services.Client.IJobService, ServiceImplementation.Implementations.ClientImplementation.JobService>();
            builder.Services.AddScoped<IFreelancerService, FreelancerService>();
            builder.Services.AddScoped<ServiceContracts.Client.IClientProfileService, ServiceImplementation.Implementations.ClientImplementation.ClientProfileService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISkillService, SkillService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IEscrowService, EscrowService>();
            builder.Services.AddHostedService<ServiceImplementation.Implementations.Contracts.DeliveryAutoCompleteService>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IGeminiService, GeminiService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // MediatR Registration
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthService).Assembly));
            // ==========================================
            // 3. JWT AUTHENTICATION SETUP
            // ==========================================
            // This tells ASP.NET how to read the token coming from React

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
                options.AddPolicy("FreelancerOnly", policy => policy.RequireRole("Freelancer"));
                options.AddPolicy("SpecialistOnly", policy => policy.RequireRole("Specialist"));
            });
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false; // Set to true in production
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
                    };
                });
            }


            // ==========================================
            // 4. CORS SETUP (Crucial for React & SignalR)
            // ==========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    b => b.SetIsOriginAllowed(origin => true) // More flexible for dev
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // Required for SignalR
            });

            builder.Services.AddSignalR();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            // builder.Services.AddOpenApiDocument();

            builder.Services.AddOpenApiDocument(config =>
            {
                config.Title = "Horr API";
                
                config.PostProcess = document =>
                {
                    document.Servers.Clear();
                    document.Servers.Add(new NSwag.OpenApiServer { Url = "https://localhost:7070", Description = "HTTPS Development" });
                    document.Servers.Add(new NSwag.OpenApiServer { Url = "http://localhost:5200", Description = "HTTP Development" });
                };

                config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}"
                });

                config.OperationProcessors.Add(
                    new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            var app = builder.Build();

            // ==========================================
            // 5. THE MIDDLEWARE PIPELINE (Order Matters!)
            // ==========================================

            app.UseMiddleware<Horr.Middleware.ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
            }

            // B.0 Global exception → ProblemDetails
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // A. Use CORS before Auth
            app.UseCors("AllowReactApp");

            // B. Turn on Authentication (Check the token)
            app.UseAuthentication();

            // C. Turn on Authorization (Check the roles)
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");
            await SeedRolesAsync(app.Services);
            app.Run();
        }

        static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // Create a new scope to retrieve scoped services
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define the specific roles you want
            string[] roleNames = { "Admin", "Client", "Freelancer", "Specialist" };

            foreach (var roleName in roleNames)
            {
                // Check if the role already exists to avoid duplicates
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}

public partial class Program { }
