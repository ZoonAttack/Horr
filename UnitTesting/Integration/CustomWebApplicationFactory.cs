using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Entities;
using Horr;

namespace UnitTesting.Integration;

using Microsoft.AspNetCore.TestHost;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");
        builder.UseSetting("JwtSettings:Key", "SuperSecretKeyForIntegrationTestingWowSoSecure!");
        builder.UseSetting("JwtSettings:Issuer", "TestIssuer");
        builder.UseSetting("JwtSettings:Audience", "TestAudience");
        builder.ConfigureTestServices(services =>
        {


            // Add AppDbContext using an in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.EnableSensitiveDataLogging();
                options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Force the default authentication scheme to Test Scheme
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
            });

            // Add the Test Authentication Handler
            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });
        });
    }
}
