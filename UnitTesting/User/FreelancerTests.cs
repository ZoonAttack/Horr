using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Linq; // Required for First(), Count(), etc.
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

// --- NAMESPACE MAPPINGS ---
// IMPORTANT: Adjust these using statements to match your project structure
using ServiceContracts.DTOs.User.Freelancer; // Your DTO namespace
using Entities.Enums; // For UserRole
using ServiceImplementation.Authentication.User; // Your service namespace
// Ensure your Entity namespaces are accessible, or add them here:
// using Entities.Users; 
// using Entities.Users.FreelancerHelpers;
// -------------------------

// NOTE: IHttpContextAccessor is not strictly needed for CreateFreelancerAsync 
// but is included here as a placeholder for the service constructor.
public interface IHttpContextAccessor { }


public class FreelancerTests
{
    // =====================================================================
    // ⚙️ SETUP UTILITIES
    // =====================================================================

    // A reusable, fully populated and valid DTO structure for Create tests
    private readonly FreelancerCreateDTO _validCreateDto = new FreelancerCreateDTO
    {
        // --- Core User Properties (Write) ---
        Email = "new.freelancer@test.com",
        FullName = "John Doe",
        Phone = "555-1234567",
        Password = "StrongPassword123",

        // --- Freelancer Profile Properties ---
        HourlyRate = 60.00M,
        Bio = "Experienced C# developer specializing in backend services.",
        Availability = "FullTime",
        YearsOfExperience = 5,
        PortfolioUrl = "https://www.johndoeportfolio.com",

        // --- Profile Collections (Must be initialized) ---
        Languages = new List<LanguageCreateDto>
        {
            new LanguageCreateDto { Name = "English", Level = "Native" }
        },
        Education = new List<EducationCreateDto>(),
        ExperienceDetails = new List<ExperienceDetailCreateDto>(),
        EmploymentHistory = new List<EmploymentCreateDto>()
    };

    #region CreateFreelancerAsync Tests

    // =====================================================================
    // 1. HAPPY PATH (SUCCESSFUL CREATION)
    // =====================================================================

    [Fact]
    public async Task CreateFreelancer_ShouldAddUserAndProfile_WhenInputIsValid()
    {
        // ARRANGE: Use a unique DB name for isolation
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        var service = new FreelancerService(context, null);

        // ACT
        var result = await service.CreateFreelancerAsync(_validCreateDto);

        // ASSERT
        // 1. Check the returned DTO
        result.Should().NotBeNull();
        result.Id.Should().NotBe(string.Empty);
        result.Email.Should().Be(_validCreateDto.Email);

        // 2. Verify Persistence: Check the database state and relationship
        var savedUser = await context.Users
            .Include(u => u.Freelancer!)
            .ThenInclude(f => f.Languages) // Eager load the child collection
            .FirstOrDefaultAsync(u => u.Id == result.Id.ToString());

        savedUser.Should().NotBeNull("User should be saved to the database.");
        savedUser!.Role.Should().Be(UserRole.Freelancer);
        savedUser.FullName.Should().Be(_validCreateDto.FullName);
        savedUser.Freelancer.Should().NotBeNull();
        savedUser.Freelancer!.HourlyRate.Should().Be(_validCreateDto.HourlyRate);

        // 3. Verify Child Collection Linkage
        savedUser.Freelancer.Languages.Should().HaveCount(1);
        savedUser.Freelancer.Languages.First().FreelancerId.Should().Be(savedUser.Id,
            "The FreelancerId in child entities must be set correctly by the service.");
    }

    // =====================================================================
    // 2. INTERNAL VALIDATION FAILURES
    // =====================================================================

    [Fact]
    public async Task CreateFreelancer_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        var service = new FreelancerService(context, null);

        // ACT & ASSERT: Expecting the service's first check to throw
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await service.CreateFreelancerAsync(null!);
        });

        // Verify database integrity
        context.Users.Count().Should().Be(0);
    }

    [Fact]
    public async Task CreateFreelancer_ShouldThrowValidationException_WhenEmailIsMissing()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        var service = new FreelancerService(context, null);

        // Create an invalid DTO by copying the valid one, but stripping the Email
        var invalidDto = new FreelancerCreateDTO
        {
            Email = "",
            FullName = _validCreateDto.FullName,
            Password = _validCreateDto.Password,
            HourlyRate = _validCreateDto.HourlyRate,
            Languages = _validCreateDto.Languages,
            Availability = _validCreateDto.Availability,
            Bio = _validCreateDto.Bio,
            YearsOfExperience = _validCreateDto.YearsOfExperience,
            PortfolioUrl = _validCreateDto.PortfolioUrl,
        };

        // ACT & ASSERT: We expect ValidationHelper.ModelValidation to throw
        await Assert.ThrowsAsync<ArgumentException>(async () => // Use ArgumentException or whatever ValidationHelper throws
        {
            await service.CreateFreelancerAsync(invalidDto);
        });

        // Verify that nothing was persisted to the database
        context.Users.Count().Should().Be(0, "No entity should be created when validation fails.");
    }

    // =====================================================================
    // 3. DATABASE CONSTRAINT FAILURES
    // =====================================================================

    [Fact]
    public async Task CreateFreelancer_ShouldThrowDbUpdateException_WhenEmailIsDuplicate()
    {
        // ARRANGE: Use SQLite in-memory database which enforces unique constraints
        // The regular in-memory database doesn't enforce unique indexes
        var dbName = Guid.NewGuid().ToString();
        using var context = DbContextUtility.CreateSqliteDbContext(dbName);
        
        // Ensure the database is created with the schema
        await context.Database.EnsureCreatedAsync();
        
        var service = new FreelancerService(context, null);
        var duplicateEmail = "conflict@test.com";

        // 1. Pre-seed the database with a user
        // Note: SQLite schema has NOT NULL constraints on CreatedAt/UpdatedAt,
        // so we must set them explicitly when seeding directly.
        context.Users.Add(new Entities.Users.User
        {
            Id = Guid.NewGuid().ToString(),
            FullName = _validCreateDto.FullName,
            Email = duplicateEmail,
            UserName = duplicateEmail,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // 2. Create the DTO with the conflicting email
        var duplicateDto = new FreelancerCreateDTO
        {
            Email = duplicateEmail,
            FullName = _validCreateDto.FullName,
            Password = _validCreateDto.Password,
            HourlyRate = 50.0M,
            Languages = new List<LanguageCreateDto>()
        };

        // ACT & ASSERT: The save attempt will cause the unique constraint violation
        await Assert.ThrowsAnyAsync<DbUpdateException>(async () =>
        {
            await service.CreateFreelancerAsync(duplicateDto);
        });

        // Ensure only the original record exists
        var count = await context.Users.CountAsync();
        count.Should().Be(1, "Only the original user should exist after the duplicate email constraint violation");
    }

    #endregion // CreateFreelancerAsync Tests

    #region UpdateFreelancerAsync Tests



    #endregion
}