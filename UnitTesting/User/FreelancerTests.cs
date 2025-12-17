using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Linq; // Required for First(), Count(), etc.
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using Moq;

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

    // Helper to create a mocked IHttpContextAccessor that simulates an authenticated freelancer
    private static Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor> CreateHttpContextAccessorMock(string userId)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth");

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var accessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        return accessorMock;
    }

    // =====================================================================
    // 1. ARGUMENT & AUTHENTICATION GUARDS
    // =====================================================================

    [Fact]
    public async Task UpdateFreelancer_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());
        var service = new FreelancerService(context, null);

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await service.UpdateFreelancerAsync(null!);
        });
    }

    [Fact]
    public async Task UpdateFreelancer_ShouldThrowUnauthorizedAccessException_WhenNoAuthenticatedUser()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        var accessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        // No HttpContext / User configured -> FindFirstValue will return null and trigger UnauthorizedAccessException
        accessorMock.Setup(a => a.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext)null!);

        var service = new FreelancerService(context, accessorMock.Object);

        var updateDto = new FreelancerUpdateDTO
        {
            FullName = "Any Name",
            Email = "any@test.com",
            Phone = "000",
            Bio = "bio"
        };

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.UpdateFreelancerAsync(updateDto);
        });
    }

    // =====================================================================
    // 2. NOT FOUND / INVALID USER CASES
    // =====================================================================

    [Fact]
    public async Task UpdateFreelancer_ShouldReturnFalse_WhenFreelancerUserNotFound()
    {
        // ARRANGE: Authenticated user ID does not exist in DB
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        var missingUserId = Guid.NewGuid().ToString();
        var accessorMock = CreateHttpContextAccessorMock(missingUserId);

        var service = new FreelancerService(context, accessorMock.Object);

        var updateDto = new FreelancerUpdateDTO
        {
            FullName = "Updated Name",
            Email = "updated@test.com",
            Phone = "111",
            Bio = "Updated bio"
        };

        // ACT
        var result = await service.UpdateFreelancerAsync(updateDto);

        // ASSERT
        result.Should().BeFalse("no freelancer user exists with the authenticated ID.");
    }

    [Fact]
    public async Task UpdateFreelancer_ShouldReturnFalse_WhenFreelancerProfileIsMissing()
    {
        // ARRANGE: User exists with Freelancer role but no Freelancer profile
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        var userId = Guid.NewGuid().ToString();
        var user = new Entities.Users.User
        {
            Id = userId,
            FullName = "Existing User",
            Email = "existing@test.com",
            UserName = "existing@test.com",
            Role = Entities.Enums.UserRole.Freelancer,
            IsDeleted = false
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var accessorMock = CreateHttpContextAccessorMock(userId);
        var service = new FreelancerService(context, accessorMock.Object);

        var updateDto = new FreelancerUpdateDTO
        {
            FullName = "Updated Name",
            Email = "updated@test.com",
            Phone = "222",
            Bio = "Updated bio"
        };

        // ACT
        var result = await service.UpdateFreelancerAsync(updateDto);

        // ASSERT
        result.Should().BeFalse("user exists but has no freelancer profile.");
    }

    // =====================================================================
    // 3. HAPPY PATH + COLLECTION RECONCILIATION
    // =====================================================================

    [Fact]
    public async Task UpdateFreelancer_ShouldUpdateScalarFields_AndReconcileCollections_WhenDataIsValid()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        var userId = Guid.NewGuid().ToString();

        var user = new Entities.Users.User
        {
            Id = userId,
            FullName = "Original Name",
            Email = "original@test.com",
            UserName = "original@test.com",
            PhoneNumber = "000",
            Role = Entities.Enums.UserRole.Freelancer,
            IsDeleted = false
        };

        var freelancer = new Entities.Users.Freelancer
        {
            UserId = userId,
            User = user,
            Bio = "Original bio",
            HourlyRate = 50m,
            Availability = "PartTime",
            YearsOfExperience = 3,
            PortfolioUrl = "https://original.com"
        };

        user.Freelancer = freelancer;

        // Seed existing collections with 2 items each
        var existingLanguage1 = new Entities.Users.FreelancerHelpers.FreelancerLanguage
        {
            FreelancerId = userId,
            Name = "English",
            Level = "Fluent"
        };
        var existingLanguage2 = new Entities.Users.FreelancerHelpers.FreelancerLanguage
        {
            FreelancerId = userId,
            Name = "Spanish",
            Level = "Intermediate"
        };

        var existingEducation1 = new Entities.Users.FreelancerHelpers.FreelancerEducation
        {
            FreelancerId = userId,
            School = "Uni A",
            DateStart = new DateTime(2010, 1, 1),
            Degree = "BSc",
            FieldOfStudy = "CS"
        };
        var existingEducation2 = new Entities.Users.FreelancerHelpers.FreelancerEducation
        {
            FreelancerId = userId,
            School = "Uni B",
            DateStart = new DateTime(2015, 1, 1),
            Degree = "MSc",
            FieldOfStudy = "IT"
        };

        var existingExperience1 = new Entities.Users.FreelancerHelpers.FreelancerExperienceDetail
        {
            FreelancerId = userId,
            Subject = "Proj 1",
            Description = "Desc 1"
        };
        var existingExperience2 = new Entities.Users.FreelancerHelpers.FreelancerExperienceDetail
        {
            FreelancerId = userId,
            Subject = "Proj 2",
            Description = "Desc 2"
        };

        var existingEmployment1 = new Entities.Users.FreelancerHelpers.FreelancerEmployment
        {
            FreelancerId = userId,
            Company = "Company A",
            City = "CityA",
            Country = "CountryA",
            Title = "Dev",
            CurrentlyWorkThere = false,
            FromDate = new DateTime(2018, 1, 1),
            ToDate = new DateTime(2020, 1, 1)
        };
        var existingEmployment2 = new Entities.Users.FreelancerHelpers.FreelancerEmployment
        {
            FreelancerId = userId,
            Company = "Company B",
            City = "CityB",
            Country = "CountryB",
            Title = "Senior Dev",
            CurrentlyWorkThere = true,
            FromDate = new DateTime(2020, 2, 1)
        };

        freelancer.Languages.Add(existingLanguage1);
        freelancer.Languages.Add(existingLanguage2);
        freelancer.Education.Add(existingEducation1);
        freelancer.Education.Add(existingEducation2);
        freelancer.ExperienceDetails.Add(existingExperience1);
        freelancer.ExperienceDetails.Add(existingExperience2);
        freelancer.EmploymentHistory.Add(existingEmployment1);
        freelancer.EmploymentHistory.Add(existingEmployment2);

        context.Users.Add(user);
        context.Freelancers.Add(freelancer);
        await context.SaveChangesAsync();

        // Prepare update DTO:
        // - Keep first item (update its fields)
        // - Remove second item (by omitting its Id)
        // - Add a new item (Id = null)

        var updateDto = new FreelancerUpdateDTO
        {
            FullName = "Updated Name",
            Email = "updated@test.com",
            Phone = "999",
            Bio = "Updated bio",
            HourlyRate = 120m,
            Availability = "FullTime",
            YearsOfExperience = 7,
            PortfolioUrl = "https://updated.com",

            Languages = new List<LanguageUpdateDto>
            {
                new LanguageUpdateDto
                {
                    Id = existingLanguage1.Id,
                    Name = "English-Updated",
                    Level = "Native"
                },
                new LanguageUpdateDto
                {
                    Id = null,
                    Name = "French",
                    Level = "Basic"
                }
            },
            Education = new List<EducationUpdateDto>
            {
                new EducationUpdateDto
                {
                    Id = existingEducation1.Id,
                    School = "Uni A Updated",
                    DateStart = existingEducation1.DateStart,
                    DateEnd = existingEducation1.DateEnd,
                    Degree = "BSc Updated",
                    FieldOfStudy = "CS Updated"
                },
                new EducationUpdateDto
                {
                    Id = null,
                    School = "Uni C",
                    DateStart = new DateTime(2021, 1, 1),
                    Degree = "PhD",
                    FieldOfStudy = "AI"
                }
            },
            ExperienceDetails = new List<ExperienceDetailUpdateDto>
            {
                new ExperienceDetailUpdateDto
                {
                    Id = existingExperience1.Id,
                    Subject = "Proj1 Updated",
                    Description = "Desc1 Updated"
                },
                new ExperienceDetailUpdateDto
                {
                    Id = null,
                    Subject = "Proj3",
                    Description = "Desc3"
                }
            },
            EmploymentHistory = new List<ServiceContracts.DTOs.User.Freelancer.EmploymentUpdateDto>
            {
                new ServiceContracts.DTOs.User.Freelancer.EmploymentUpdateDto
                {
                    Id = existingEmployment1.Id,
                    Company = "Company A Updated",
                    City = "CityA Updated",
                    Country = "CountryA Updated",
                    Title = "Lead Dev",
                    CurrentlyWorkThere = false,
                    FromDate = existingEmployment1.FromDate,
                    ToDate = existingEmployment1.ToDate
                },
                new ServiceContracts.DTOs.User.Freelancer.EmploymentUpdateDto
                {
                    Id = null,
                    Company = "Company C",
                    City = "CityC",
                    Country = "CountryC",
                    Title = "Architect",
                    CurrentlyWorkThere = true,
                    FromDate = new DateTime(2024, 1, 1)
                }
            }
        };

        var accessorMock = CreateHttpContextAccessorMock(userId);
        var service = new FreelancerService(context, accessorMock.Object);

        // ACT
        var result = await service.UpdateFreelancerAsync(updateDto);

        // ASSERT - result
        result.Should().BeTrue();

        // Reload user with navigation properties
        var updatedUser = await context.Users
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.Languages)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.Education)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.ExperienceDetails)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.EmploymentHistory)
            .FirstAsync(u => u.Id == userId);

        // Scalar fields
        updatedUser.FullName.Should().Be(updateDto.FullName);
        updatedUser.Email.Should().Be(updateDto.Email);
        updatedUser.PhoneNumber.Should().Be(updateDto.Phone);

        updatedUser.Freelancer.Bio.Should().Be(updateDto.Bio);
        updatedUser.Freelancer.HourlyRate.Should().Be(updateDto.HourlyRate);
        updatedUser.Freelancer.Availability.Should().Be(updateDto.Availability);
        updatedUser.Freelancer.YearsOfExperience.Should().Be(updateDto.YearsOfExperience);
        updatedUser.Freelancer.PortfolioUrl.Should().Be(updateDto.PortfolioUrl);

        // Languages reconciliation: 2 items (1 updated existing + 1 new)
        updatedUser.Freelancer.Languages.Should().HaveCount(2);
        updatedUser.Freelancer.Languages
            .Any(l => l.Id == existingLanguage1.Id && l.Name == "English-Updated" && l.Level == "Native")
            .Should().BeTrue("existing language should be updated.");
        updatedUser.Freelancer.Languages
            .Any(l => l.Name == "French" && l.Level == "Basic")
            .Should().BeTrue("new language should be added.");

        // Education reconciliation
        updatedUser.Freelancer.Education.Should().HaveCount(2);
        updatedUser.Freelancer.Education
            .Any(e => e.Id == existingEducation1.Id && e.School == "Uni A Updated" && e.Degree == "BSc Updated")
            .Should().BeTrue();
        updatedUser.Freelancer.Education
            .Any(e => e.School == "Uni C" && e.Degree == "PhD")
            .Should().BeTrue();

        // Experience reconciliation
        updatedUser.Freelancer.ExperienceDetails.Should().HaveCount(2);
        updatedUser.Freelancer.ExperienceDetails
            .Any(e => e.Id == existingExperience1.Id && e.Subject == "Proj1 Updated")
            .Should().BeTrue();
        updatedUser.Freelancer.ExperienceDetails
            .Any(e => e.Subject == "Proj3" && e.Description == "Desc3")
            .Should().BeTrue();

        // Employment reconciliation
        updatedUser.Freelancer.EmploymentHistory.Should().HaveCount(2);
        updatedUser.Freelancer.EmploymentHistory
            .Any(e => e.Id == existingEmployment1.Id && e.Company == "Company A Updated" && e.Title == "Lead Dev")
            .Should().BeTrue();
        updatedUser.Freelancer.EmploymentHistory
            .Any(e => e.Company == "Company C" && e.Title == "Architect")
            .Should().BeTrue();

        // Ensure the "second" original items were deleted (not present anymore)
        updatedUser.Freelancer.Languages
            .Any(l => l.Id == existingLanguage2.Id).Should().BeFalse();
        updatedUser.Freelancer.Education
            .Any(e => e.Id == existingEducation2.Id).Should().BeFalse();
        updatedUser.Freelancer.ExperienceDetails
            .Any(e => e.Id == existingExperience2.Id).Should().BeFalse();
        updatedUser.Freelancer.EmploymentHistory
            .Any(e => e.Id == existingEmployment2.Id).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFreelancer_ShouldClearCollections_WhenUpdateDtoCollectionsAreEmpty()
    {
        // ARRANGE
        using var context = DbContextUtility.CreateDbContext(Guid.NewGuid().ToString());

        var userId = Guid.NewGuid().ToString();

        var user = new Entities.Users.User
        {
            Id = userId,
            FullName = "Original Name",
            Email = "original@test.com",
            UserName = "original@test.com",
            PhoneNumber = "000",
            Role = Entities.Enums.UserRole.Freelancer,
            IsDeleted = false
        };

        var freelancer = new Entities.Users.Freelancer
        {
            UserId = userId,
            User = user,
            Bio = "Original bio",
            HourlyRate = 50m,
            Availability = "PartTime",
            YearsOfExperience = 3,
            PortfolioUrl = "https://original.com"
        };

        user.Freelancer = freelancer;

        freelancer.Languages.Add(new Entities.Users.FreelancerHelpers.FreelancerLanguage
        {
            FreelancerId = userId,
            Name = "English",
            Level = "Fluent"
        });

        freelancer.Education.Add(new Entities.Users.FreelancerHelpers.FreelancerEducation
        {
            FreelancerId = userId,
            School = "Uni A",
            DateStart = new DateTime(2010, 1, 1),
            Degree = "BSc",
            FieldOfStudy = "CS"
        });

        freelancer.ExperienceDetails.Add(new Entities.Users.FreelancerHelpers.FreelancerExperienceDetail
        {
            FreelancerId = userId,
            Subject = "Proj 1",
            Description = "Desc 1"
        });

        freelancer.EmploymentHistory.Add(new Entities.Users.FreelancerHelpers.FreelancerEmployment
        {
            FreelancerId = userId,
            Company = "Company A",
            City = "CityA",
            Country = "CountryA",
            Title = "Dev",
            CurrentlyWorkThere = false,
            FromDate = new DateTime(2018, 1, 1),
            ToDate = new DateTime(2020, 1, 1)
        });

        context.Users.Add(user);
        context.Freelancers.Add(freelancer);
        await context.SaveChangesAsync();

        var updateDto = new FreelancerUpdateDTO
        {
            FullName = "Updated Name",
            Email = "updated@test.com",
            Phone = "999",
            Bio = "Updated bio",
            HourlyRate = 60m,
            Availability = "FullTime",
            YearsOfExperience = 5,
            PortfolioUrl = "https://updated.com",

            Languages = new List<LanguageUpdateDto>(),
            Education = new List<EducationUpdateDto>(),
            ExperienceDetails = new List<ExperienceDetailUpdateDto>(),
            EmploymentHistory = new List<ServiceContracts.DTOs.User.Freelancer.EmploymentUpdateDto>()
        };

        var accessorMock = CreateHttpContextAccessorMock(userId);
        var service = new FreelancerService(context, accessorMock.Object);

        // ACT
        var result = await service.UpdateFreelancerAsync(updateDto);

        // ASSERT
        result.Should().BeTrue();

        var updatedUser = await context.Users
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.Languages)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.Education)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.ExperienceDetails)
            .Include(u => u.Freelancer)
                .ThenInclude(f => f.EmploymentHistory)
            .FirstAsync(u => u.Id == userId);

        updatedUser.Freelancer.Languages.Should().BeEmpty();
        updatedUser.Freelancer.Education.Should().BeEmpty();
        updatedUser.Freelancer.ExperienceDetails.Should().BeEmpty();
        updatedUser.Freelancer.EmploymentHistory.Should().BeEmpty();
    }

    #endregion
}