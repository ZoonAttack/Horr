using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;
using ServiceImplementation.Implementations.Marketplace;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using Entities;
using Entities.Marketplace;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTesting.Marketplace
{
    public class ServiceGalleryUploadTests
    {
        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private Mock<IFormFile> CreateMockFile(string fileName, long length)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(length);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream(new byte[length]));
            return fileMock;
        }

        [Fact]
        public async Task UploadGallery_GifImage_ShouldThrowValidationException()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem { Id = "s1", FreelancerId = "f1", Title = "Service", Description = new string('a', 120) };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var gif = CreateMockFile("test.gif", 1024).Object;

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", Images: new List<IFormFile> { gif });

            // ACT
            var act = () => handler.Handle(command, CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Images") && e.Contains(".gif"));
        }

        [Fact]
        public async Task UploadGallery_LargeImage_ShouldThrowValidationException()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem { Id = "s1", FreelancerId = "f1", Title = "Service", Description = new string('a', 120) };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var largeImage = CreateMockFile("test.jpg", 11 * 1024 * 1024).Object; // 11MB

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", Images: new List<IFormFile> { largeImage });

            // ACT
            var act = () => handler.Handle(command, CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Images") && e.Contains("10MB"));
        }

        [Fact]
        public async Task UploadGallery_TooManyImages_ShouldThrowValidationException()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem { Id = "s1", FreelancerId = "f1", Title = "Service", Description = new string('a', 120) };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var images = new List<IFormFile>();
            for (int i = 0; i < 16; i++)
            {
                images.Add(CreateMockFile($"test{i}.jpg", 1024).Object);
            }

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", Images: images);

            // ACT
            var act = () => handler.Handle(command, CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Images") && e.Contains("Maximum 15 images allowed."));
        }

        [Fact]
        public async Task UploadGallery_MovVideo_ShouldThrowValidationException()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem { Id = "s1", FreelancerId = "f1", Title = "Service", Description = new string('a', 120) };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var mov = CreateMockFile("test.mov", 1024).Object;

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", Video: mov);

            // ACT
            var act = () => handler.Handle(command, CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Video") && e.Contains(".mp4"));
        }

        [Fact]
        public async Task UploadGallery_TooManyDocuments_ShouldThrowValidationException()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem { Id = "s1", FreelancerId = "f1", Title = "Service", Description = new string('a', 120) };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var docs = new List<IFormFile>();
            for (int i = 0; i < 4; i++)
            {
                docs.Add(CreateMockFile($"test{i}.pdf", 1024).Object);
            }

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", Documents: docs);

            // ACT
            var act = () => handler.Handle(command, CancellationToken.None);

            // ASSERT
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(e => e.Contains("Documents") && e.Contains("Maximum 3 documents allowed."));
        }

        [Fact]
        public async Task UploadGallery_SetNewCover_ShouldUnsetPreviousCover()
        {
            // ARRANGE
            var context = GetContext();
            var service = new ServiceCatalogItem 
            { 
                Id = "s1", 
                FreelancerId = "f1", 
                Title = "Service",
                Description = new string('a', 120),
                GalleryFiles = new List<ServiceGalleryFile>
                {
                    new ServiceGalleryFile { Id = "g1", FileUrl = "url1", FileType = ServiceGalleryFileType.Image, IsCover = true }
                }
            };
            context.ServiceCatalogItems.Add(service);
            await context.SaveChangesAsync();

            var handler = new UploadServiceGalleryFilesCommandHandler(context);
            var newImage = CreateMockFile("new_cover.jpg", 1024).Object;

            var command = new UploadServiceGalleryFilesCommand("s1", "f1", 
                Images: new List<IFormFile> { newImage }, 
                CoverImageFileName: "new_cover.jpg");

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            var updatedService = await context.ServiceCatalogItems.Include(s => s.GalleryFiles).FirstAsync(s => s.Id == "s1");
            updatedService.GalleryFiles.Count(f => f.IsCover).Should().Be(1);
            updatedService.GalleryFiles.First(f => f.FileUrl.Contains("new_cover")).IsCover.Should().BeTrue();
            updatedService.GalleryFiles.First(f => f.Id == "g1").IsCover.Should().BeFalse();
        }
    }
}
