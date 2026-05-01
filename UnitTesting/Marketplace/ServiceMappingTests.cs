using System.Collections.Generic;
using System.Linq;
using Entities.Marketplace;
using Entities.Enums;
using ServiceContracts.DTOs.Services;
using FluentAssertions;
using Xunit;

namespace UnitTesting.Marketplace
{
    public class ServiceMappingTests
    {
        [Fact]
        public void ToDto_ShouldMapNestedCollectionsCorrectly()
        {
            // ARRANGE
            var service = new ServiceCatalogItem
            {
                Id = "s1",
                Title = "Test Service",
                Description = "This is a long description for the test service to meet the min length requirement.",
                Steps = new List<ServiceStep>
                {
                    new ServiceStep { Id = "st1", Title = "Step 1", StepNumber = 1 },
                    new ServiceStep { Id = "st2", Title = "Step 2", StepNumber = 2 }
                },
                GalleryFiles = new List<ServiceGalleryFile>
                {
                    new ServiceGalleryFile { Id = "g1", FileUrl = "url1", IsCover = true },
                    new ServiceGalleryFile { Id = "g2", FileUrl = "url2", IsCover = false },
                    new ServiceGalleryFile { Id = "g3", FileUrl = "url3", IsCover = false }
                },
                Faqs = new List<ServiceFaq>
                {
                    new ServiceFaq { Id = "f1", Question = "Q1", Answer = "A1" }
                }
            };

            // ACT
            var dto = service.ToDto();

            // ASSERT
            dto.Should().NotBeNull();
            dto.Steps.Should().HaveCount(2);
            dto.GalleryFiles.Should().HaveCount(3);
            dto.Faqs.Should().HaveCount(1);
        }

        [Fact]
        public void ToDto_ShouldMapIsCoverFlagCorrectly()
        {
            // ARRANGE
            var file = new ServiceGalleryFile
            {
                Id = "g1",
                FileUrl = "url1",
                IsCover = true,
                FileType = ServiceGalleryFileType.Image
            };

            // ACT
            var dto = file.ToDto();

            // ASSERT
            dto.Should().NotBeNull();
            dto.IsCover.Should().BeTrue();
        }

        [Fact]
        public void ToDto_ShouldMapStepNumberCorrectly()
        {
            // ARRANGE
            var step = new ServiceStep
            {
                Id = "st2",
                Title = "Step 2",
                StepNumber = 2
            };

            // ACT
            var dto = step.ToDto();

            // ASSERT
            dto.Should().NotBeNull();
            dto.StepNumber.Should().Be(2);
        }

        [Fact]
        public void ToDto_ShouldMapAttributesCorrectly()
        {
            // ARRANGE
            var attr = new ServiceAttribute
            {
                Id = "a1",
                Value = "Tag1"
            };

            // ACT
            var dto = attr.ToDto();

            // ASSERT
            dto.Should().NotBeNull();
            dto.Value.Should().Be("Tag1");
        }
    }
}
