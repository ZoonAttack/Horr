using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs.Services;
using System.Text.Json.Serialization;
using Xunit;

namespace UnitTesting.Integration
{
    public class ServicesControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _jsonOptions;

        public ServicesControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        private void SetUser(string userId, string role = "Freelancer")
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-UserRole");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
            _client.DefaultRequestHeaders.Add("X-Test-UserRole", role);
        }

        [Fact]
        public async Task CreateService_WithValidPayload_Returns201AndDto()
        {
            // Arrange
            SetUser("freelancer-1");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("High Quality Logo Design"), "Title");
            content.Add(new StringContent("I will design a professional logo for your brand with multiple revisions and source files included in the final delivery package."), "Description");
            content.Add(new StringContent("100"), "Price");
            content.Add(new StringContent("3 days"), "DeliveryTime");
            
            // Nested objects in form-data
            content.Add(new StringContent("100"), "Pricing.PriceFrom");
            content.Add(new StringContent("500"), "Pricing.PriceTo");
            content.Add(new StringContent("3"), "Pricing.DeliveryDays");
            content.Add(new StringContent("5"), "Pricing.RevisionsIncluded");

            content.Add(new StringContent("What is your brand name?"), "Requirements[0].Question");
            content.Add(new StringContent("true"), "Requirements[0].IsRequired");

            content.Add(new StringContent("1"), "Steps[0].StepNumber");
            content.Add(new StringContent("Briefing"), "Steps[0].Title");
            content.Add(new StringContent("We discuss your needs"), "Steps[0].Description");

            // Files
            var imageContent = new ByteArrayContent(new byte[] { 0x1, 0x2, 0x3 });
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            content.Add(imageContent, "images", "logo.jpg");
            content.Add(new StringContent("logo.jpg"), "coverImageFileName");

            // Act
            var response = await _client.PostAsync("/api/services", content);

            // Assert
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Expected 201, but got {response.StatusCode}. Body: {errorBody}");
            }

            var dto = await response.Content.ReadFromJsonAsync<ServiceCatalogItemDto>(_jsonOptions);
            dto.Should().NotBeNull();
            dto.Title.Should().Be("High Quality Logo Design");
            dto.Status.Should().Be("UnderReview");
        }

        [Fact]
        public async Task CreateService_WithShortDescription_Returns400()
        {
            // Arrange
            SetUser("freelancer-1");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("Short Title"), "Title");
            content.Add(new StringContent("Too short description"), "Description");
            content.Add(new StringContent("Q"), "Requirements[0].Question");
            content.Add(new StringContent("1"), "Steps[0].StepNumber");

            // Act
            var response = await _client.PostAsync("/api/services", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            var allErrors = string.Join(" ", problem.Errors.SelectMany(e => e.Value));
            allErrors.Should().Contain("Description must be at least 120 characters");
        }

        [Fact]
        public async Task CreateService_WithTooManyImages_Returns400()
        {
            // Arrange
            SetUser("freelancer-1");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("Valid Title"), "Title");
            content.Add(new StringContent(new string('a', 130)), "Description");
            content.Add(new StringContent("Q"), "Requirements[0].Question");
            content.Add(new StringContent("1"), "Steps[0].StepNumber");

            for (int i = 0; i < 16; i++)
            {
                var image = new ByteArrayContent(new byte[] { 0x1 });
                image.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                content.Add(image, "images", $"img{i}.png");
            }

            // Act
            var response = await _client.PostAsync("/api/services", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            var allErrors = string.Join(" ", problem.Errors.SelectMany(e => e.Value));
            allErrors.Should().Contain("Maximum 15 images allowed");
        }

        [Fact]
        public async Task UpdateService_WithNewSteps_Returns200AndExactlyThreeSteps()
        {
            // Arrange - Create first
            SetUser("freelancer-update");
            var createContent = new MultipartFormDataContent();
            createContent.Add(new StringContent("High Quality Logo Design"), "Title");
            createContent.Add(new StringContent(new string('a', 130)), "Description");
            createContent.Add(new StringContent("100"), "Price");
            createContent.Add(new StringContent("3 days"), "DeliveryTime");
            createContent.Add(new StringContent("What are your requirements for this project?"), "Requirements[0].Question");
            createContent.Add(new StringContent("true"), "Requirements[0].IsRequired");
            createContent.Add(new StringContent("1"), "Steps[0].StepNumber");
            createContent.Add(new StringContent("Initial Step Title"), "Steps[0].Title");
            createContent.Add(new StringContent("Initial Step Description"), "Steps[0].Description");
            
            var createResponse = await _client.PostAsync("/api/services", createContent);
            var initialDto = await createResponse.Content.ReadFromJsonAsync<ServiceCatalogItemDto>(_jsonOptions);

            // Act - Update with 3 steps
            var updateContent = new MultipartFormDataContent();
            updateContent.Add(new StringContent(initialDto.Id), "Id");
            updateContent.Add(new StringContent("Update Test Service"), "Title");
            updateContent.Add(new StringContent(new string('a', 130)), "Description");
            updateContent.Add(new StringContent("What are your updated requirements?"), "Requirements[0].Question");

            for (int i = 1; i <= 3; i++)
            {
                updateContent.Add(new StringContent(i.ToString()), $"Steps[{i - 1}].StepNumber");
                updateContent.Add(new StringContent($"New Step {i}"), $"Steps[{i - 1}].Title");
                updateContent.Add(new StringContent($"Desc {i}"), $"Steps[{i - 1}].Description");
            }

            var response = await _client.PutAsync($"/api/services/{initialDto.Id}", updateContent);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Expected 200, but got {response.StatusCode}. Body: {errorBody}");
            }

            var updatedDto = await response.Content.ReadFromJsonAsync<ServiceCatalogItemDto>(_jsonOptions);
            updatedDto.Steps.Should().HaveCount(3);
            updatedDto.Steps.Select(s => s.Title).Should().Contain(new[] { "New Step 1", "New Step 2", "New Step 3" });
        }

        [Fact]
        public async Task DeleteService_SoftDeletesAndExcludesFromList()
        {
            // Arrange
            SetUser("freelancer-delete");
            var createContent = new MultipartFormDataContent();
            createContent.Add(new StringContent("Delete Test Service"), "Title");
            createContent.Add(new StringContent(new string('a', 130)), "Description");
            createContent.Add(new StringContent("100"), "Price");
            createContent.Add(new StringContent("3 days"), "DeliveryTime");
            createContent.Add(new StringContent("What are your requirements for this project?"), "Requirements[0].Question");
            createContent.Add(new StringContent("true"), "Requirements[0].IsRequired");
            createContent.Add(new StringContent("1"), "Steps[0].StepNumber");
            createContent.Add(new StringContent("Initial Step Title"), "Steps[0].Title");
            createContent.Add(new StringContent("Initial Step Description"), "Steps[0].Description");
            
            var createResponse = await _client.PostAsync("/api/services", createContent);
            var dto = await createResponse.Content.ReadFromJsonAsync<ServiceCatalogItemDto>(_jsonOptions);

            // Act - Delete
            var deleteResponse = await _client.DeleteAsync($"/api/services/{dto.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Assert - Excluded from list
            var listResponse = await _client.GetAsync("/api/services/my-services");
            var grouped = await listResponse.Content.ReadFromJsonAsync<ServiceGroupedDto>(_jsonOptions);
            grouped.UnderReview.Should().NotContain(s => s.Id == dto.Id);
            grouped.Approved.Should().NotContain(s => s.Id == dto.Id);
        }

        [Fact]
        public async Task GetUnknownService_Returns404()
        {
            // Arrange
            SetUser("any-user");

            // Act
            var response = await _client.GetAsync("/api/services/unknown-id");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Title.Should().Be("Not Found");
        }

        [Fact]
        public async Task GetMyServices_ReturnsCorrectStructure()
        {
            // Arrange
            SetUser("freelancer-list");
            // Create one UnderReview
            var content1 = new MultipartFormDataContent();
            content1.Add(new StringContent("Service 1"), "Title");
            content1.Add(new StringContent(new string('a', 130)), "Description");
            content1.Add(new StringContent("100"), "Price");
            content1.Add(new StringContent("3 days"), "DeliveryTime");
            content1.Add(new StringContent("What are your requirements for this project?"), "Requirements[0].Question");
            content1.Add(new StringContent("true"), "Requirements[0].IsRequired");
            content1.Add(new StringContent("1"), "Steps[0].StepNumber");
            content1.Add(new StringContent("Initial Step Title"), "Steps[0].Title");
            content1.Add(new StringContent("Initial Step Description"), "Steps[0].Description");
            await _client.PostAsync("/api/services", content1);

            // Act
            var response = await _client.GetAsync("/api/services/my-services");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var grouped = await response.Content.ReadFromJsonAsync<ServiceGroupedDto>(_jsonOptions);
            grouped.Should().NotBeNull();
            grouped.UnderReview.Should().NotBeNull();
            grouped.Approved.Should().NotBeNull();
            grouped.UnderReview.Should().Contain(s => s.Title == "Service 1");
        }
    }
}
