using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace UnitTesting.Integration
{
    public class TestAuthHandlerOptions : AuthenticationSchemeOptions
    {
        public string DefaultUserId { get; set; } = "default-test-user";
        public string DefaultUserRole { get; set; } = "Freelancer";
    }

    public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
    {
        public TestAuthHandler(IOptionsMonitor<TestAuthHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userId = Context.Request.Headers["X-Test-UserId"].FirstOrDefault() 
                         ?? Context.Request.Headers["X-UserId"].FirstOrDefault()
                         ?? Options.DefaultUserId;
            var userRole = Context.Request.Headers["X-Test-UserRole"].FirstOrDefault() 
                           ?? Context.Request.Headers["X-Role"].FirstOrDefault()
                           ?? Options.DefaultUserRole;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, userRole),
                new Claim("UserRole", userRole)
            };

            var identity = new ClaimsIdentity(claims, "TestScheme", ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // 1. Database
                var dbDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || 
                                                       d.ServiceType == typeof(AppDbContext)).ToList();
                foreach (var d in dbDescriptors) services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // 2. Add Test Authentication (Program.cs skips production auth in Testing env)
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultScheme = "TestScheme";
                }).AddScheme<TestAuthHandlerOptions, TestAuthHandler>("TestScheme", options => { });

                // 3. Authorization (We let the real authorization run now)
                services.AddAuthorization();
            });
        }
    }
}

