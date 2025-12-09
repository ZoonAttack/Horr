using Entities;
using Entities.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts;
using ServiceImplementation.Authentication;
using Services.Implementations;
using Services.Interfaces;
using System.Text;

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
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            // ==========================================
            // 3. JWT AUTHENTICATION SETUP
            // ==========================================
            // This tells ASP.NET how to read the token coming from React
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

            // ==========================================
            // 4. CORS SETUP (Crucial for React)
            // ==========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    b => b.WithOrigins("http://localhost:3000") // Your React URL
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            builder.Services.AddControllers();
            builder.Services.AddOpenApiDocument();

            var app = builder.Build();

            // ==========================================
            // 5. THE MIDDLEWARE PIPELINE (Order Matters!)
            // ==========================================

            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
            }

            app.UseHttpsRedirection();

            // A. Use CORS before Auth
            app.UseCors("AllowReactApp");

            // B. Turn on Authentication (Check the token)
            app.UseAuthentication();

            // C. Turn on Authorization (Check the roles)
            app.UseAuthorization();

            app.MapControllers();
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
