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
using Microsoft.EntityFrameworkCore;

namespace UnitTesting.Marketplace
{
    public class ServiceValidationTests
    {
        private CreateServiceCommandHandler GetHandler()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);
            return new CreateServiceCommandHandler(context);
        }

        private ServiceCreateDTO GetValidDto()
        {
            return new ServiceCreateDTO
            {
                FreelancerId = "f1",
                Title = "Valid Title",
                Description = new string('a', 120),
                Requirements = new List<ServiceRequirementDto> { new ServiceRequirementDto { Question = "A requirement that is long enough" } },
                Steps = new List<ServiceStepDto> { new ServiceStepDto { Title = "Step 1", StepNumber = 1 } }
            };
        }

        [Fact]
        public async Task CreateService_EmptyTitle_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Title = "";

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Title"));
        }

        [Fact]
        public async Task CreateService_DescriptionTooShort_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Description = new string('a', 119);

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Description"));
        }

        [Fact]
        public async Task CreateService_DescriptionExactly120_ShouldPass()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Description = new string('a', 120);

            // ACT
            var result = await handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateService_FourAttributes_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Attributes = new List<ServiceAttributeDto>
            {
                new ServiceAttributeDto { Value = "1" },
                new ServiceAttributeDto { Value = "2" },
                new ServiceAttributeDto { Value = "3" },
                new ServiceAttributeDto { Value = "4" }
            };

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Attributes"));
        }

        [Fact]
        public async Task CreateService_ThreeAttributes_ShouldPass()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Attributes = new List<ServiceAttributeDto>
            {
                new ServiceAttributeDto { Value = "1" },
                new ServiceAttributeDto { Value = "2" },
                new ServiceAttributeDto { Value = "3" }
            };

            // ACT
            var result = await handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateService_SixFaqs_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Faqs = new List<ServiceFaqDto>
            {
                new ServiceFaqDto { Question = "1", Answer = "1" },
                new ServiceFaqDto { Question = "2", Answer = "2" },
                new ServiceFaqDto { Question = "3", Answer = "3" },
                new ServiceFaqDto { Question = "4", Answer = "4" },
                new ServiceFaqDto { Question = "5", Answer = "5" },
                new ServiceFaqDto { Question = "6", Answer = "6" }
            };

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Faqs"));
        }

        [Fact]
        public async Task CreateService_EmptyRequirements_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Requirements = new List<ServiceRequirementDto>();

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Requirements"));
        }

        [Fact]
        public async Task CreateService_EmptySteps_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Steps = new List<ServiceStepDto>();

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Steps"));
        }

        [Fact]
        public async Task CreateService_ShortStepTitle_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Steps = new List<ServiceStepDto> { new ServiceStepDto { Title = "12", StepNumber = 1 } };

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Steps[0].Title"));
        }

        [Fact]
        public async Task CreateService_ShortRequirementQuestion_ShouldThrowValidationException()
        {
            // ARRANGE
            var handler = GetHandler();
            var dto = GetValidDto();
            dto.Requirements = new List<ServiceRequirementDto> { new ServiceRequirementDto { Question = "123456789" } };

            // ACT
            var act = () => handler.Handle(new CreateServiceCommand(dto), CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Requirements[0].Question"));
        }
    }
}
