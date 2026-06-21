using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using ServiceImplementation.Implementations.Marketplace;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using Entities;
using Entities.Marketplace;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts.Currency;

namespace UnitTesting.Marketplace
{
    public class ServiceHandlerTests
    {
        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateService_ShouldPersistAllChildren_AndSetStatusToUnderReview()
        {
            // ARRANGE
            var context = GetContext();
            var handler = new CreateServiceCommandHandler(context);
            var dto = new ServiceCreateDTO
            {
                FreelancerId = "f1",
                Title = "New Service",
                Description = new string('a', 120),
                Price = 100,
                Pricing = new ServicePricingDto { PriceFrom = 100, PriceTo = 200, DeliveryDays = 5 },
                Requirements = new List<ServiceRequirementDto> { new ServiceRequirementDto { Question = "Question 1", IsRequired = true } },
                Steps = new List<ServiceStepDto> { new ServiceStepDto { Title = "Step 1", Description = "Desc 1", StepNumber = 1 } },
                Faqs = new List<ServiceFaqDto> { new ServiceFaqDto { Question = "Q1", Answer = "A1" } },
                Attributes = new List<ServiceAttributeDto> { new ServiceAttributeDto { Value = "Attr1" } }
            };

            var command = new CreateServiceCommand(dto);

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            var service = await context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .FirstAsync(s => s.Id == result.Id);

            service.Status.Should().Be(ServiceStatus.UnderReview);
            service.Pricing.Should().NotBeNull();
            service.Requirements.Should().HaveCount(1);
            service.Steps.Should().HaveCount(1);
            service.Faqs.Should().HaveCount(1);
            service.Attributes.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateService_WithStatusApprovedInPayload_ShouldStillBeUnderReview()
        {
            // ARRANGE
            var context = GetContext();
            var handler = new CreateServiceCommandHandler(context);
            // Note: ServiceCreateDTO doesn't even have a Status field, 
            // but we ensure the mapper/handler enforces UnderReview.
            var dto = new ServiceCreateDTO
            {
                FreelancerId = "f1",
                Title = "New Service",
                Description = new string('a', 120),
                Requirements = new List<ServiceRequirementDto> { new ServiceRequirementDto { Question = "Question 1" } },
                Steps = new List<ServiceStepDto> { new ServiceStepDto { Title = "Step 1" } }
            };

            var command = new CreateServiceCommand(dto);

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            var service = await context.ServiceCatalogItems.FirstAsync(s => s.Id == result.Id);
            service.Status.Should().Be(ServiceStatus.UnderReview);
        }

        [Fact]
        public async Task UpdateService_ShouldReplaceChildCollections()
        {
            // ARRANGE
            var context = GetContext();
            var serviceId = "s1";
            var freelancerId = "f1";
            var service = new ServiceCatalogItem
            {
                Id = serviceId,
                FreelancerId = freelancerId,
                Title = "Old Title",
                Description = new string('a', 120),
                Steps = new List<ServiceStep>
                {
                    new ServiceStep { Id = "step1", Title = "Old Step 1", StepNumber = 1 },
                    new ServiceStep { Id = "step2", Title = "Old Step 2", StepNumber = 2 }
                }
            };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UpdateServiceCommandHandler(context);
            var updateDto = new ServiceUpdateDTO
            {
                Id = serviceId,
                FreelancerId = freelancerId,
                Title = "New Title",
                Description = new string('b', 120),
                Requirements = new List<ServiceRequirementDto> { new ServiceRequirementDto { Question = "New Req 1" } },
                Steps = new List<ServiceStepDto>
                {
                    new ServiceStepDto { Title = "New Step 1", StepNumber = 1 },
                    new ServiceStepDto { Title = "New Step 2", StepNumber = 2 },
                    new ServiceStepDto { Title = "New Step 3", StepNumber = 3 }
                }
            };

            var command = new UpdateServiceCommand(updateDto);

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            var updatedService = await context.ServiceCatalogItems
                .Include(s => s.Steps)
                .FirstAsync(s => s.Id == serviceId);

            updatedService.Steps.Should().HaveCount(3);
            updatedService.Steps.Should().Contain(s => s.Title == "New Step 3");
            updatedService.Steps.Should().NotContain(s => s.Title == "Old Step 1");
            
            // Check that old rows are actually gone from DB
            var allSteps = await context.ServiceSteps.Where(s => s.ServiceId == serviceId).ToListAsync();
            allSteps.Should().HaveCount(3);
        }

        [Fact]
        public async Task DeleteService_ShouldSoftDelete_AndBeExcludedFromQueries()
        {
            // ARRANGE
            var context = GetContext();
            var serviceId = "s1";
            var freelancerId = "f1";
            var service = new ServiceCatalogItem
            {
                Id = serviceId,
                FreelancerId = freelancerId,
                Title = "Service to Delete",
                Description = new string('a', 120),
                Status = ServiceStatus.Approved
            };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var deleteHandler = new DeleteServiceCommandHandler(context);
            var queryHandler = new GetMyServicesQueryHandler(context, new Mock<ICurrencyConverterService>().Object);

            // ACT
            await deleteHandler.Handle(new DeleteServiceCommand(serviceId, freelancerId), CancellationToken.None);
            
            // ASSERT
            // 1. Check entity state
            context.ChangeTracker.Clear();
            var deletedService = await context.ServiceCatalogItems.IgnoreQueryFilters().FirstAsync(s => s.Id == serviceId);
            deletedService.IsDeleted.Should().BeTrue();

            // 2. Check query exclusion
            var queryResult = await queryHandler.Handle(new GetMyServicesQuery(freelancerId), CancellationToken.None);
            queryResult.Approved.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMyServices_ShouldGroupCorrectly()
        {
            // ARRANGE
            var context = GetContext();
            var f1 = "f1";
            context.ServiceCatalogItems.AddRange(new List<ServiceCatalogItem>
            {
                new ServiceCatalogItem { Id = "1", FreelancerId = f1, Title = "A1", Description = new string('a', 120), Status = ServiceStatus.Approved },
                new ServiceCatalogItem { Id = "2", FreelancerId = f1, Title = "A2", Description = new string('a', 120), Status = ServiceStatus.Approved },
                new ServiceCatalogItem { Id = "3", FreelancerId = f1, Title = "A3", Description = new string('a', 120), Status = ServiceStatus.Approved },
                new ServiceCatalogItem { Id = "4", FreelancerId = f1, Title = "U1", Description = new string('a', 120), Status = ServiceStatus.UnderReview },
                new ServiceCatalogItem { Id = "5", FreelancerId = f1, Title = "U2", Description = new string('a', 120), Status = ServiceStatus.UnderReview }
            });
            await context.SaveChangesAsync();

            var handler = new GetMyServicesQueryHandler(context, new Mock<ICurrencyConverterService>().Object);

            // ACT
            var result = await handler.Handle(new GetMyServicesQuery(f1), CancellationToken.None);

            // ASSERT
            result.Approved.Should().HaveCount(3);
            result.UnderReview.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetServiceById_UnknownId_ShouldThrowNotFoundException()
        {
            // ARRANGE
            var context = GetContext();
            var handler = new GetServiceByIdQueryHandler(context, new Mock<ICurrencyConverterService>().Object);

            // ACT
            var act = () => handler.Handle(new GetServiceByIdQuery("unknown", "f1"), CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetServiceById_OtherUserId_ShouldThrowNotFoundException()
        {
            // ARRANGE
            var context = GetContext();
            context.ServiceCatalogItems.Add(new ServiceCatalogItem 
            { 
                Id = "s1", 
                FreelancerId = "owner", 
                Title = "Service", 
                Description = new string('a', 120) 
            });
            await context.SaveChangesAsync();

            var handler = new GetServiceByIdQueryHandler(context, new Mock<ICurrencyConverterService>().Object);

            // ACT
            var act = () => handler.Handle(new GetServiceByIdQuery("s1", "other"), CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
