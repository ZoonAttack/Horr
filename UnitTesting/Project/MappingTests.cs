using System;
using Entities.Enums;
using Entities.Project;
using Entities.Review;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Reviews;
using Xunit;

namespace UnitTesting.Project
{
    public class MappingTests
    {
        [Fact]
        public void Contract_ToDto_MapsAllFieldsCorrectly_WithNullClosedAt()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1,
                ProposalId = 10,
                ClientId = "client-1",
                FreelancerId = "free-1",
                AgreedRate = 150.5m,
                Status = ContractStatus.Active,
                StartedAt = new DateTime(2026, 1, 1),
                ClosedAt = null
            };

            // Act
            var dto = contract.ToDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(contract.Id, dto.Id);
            Assert.Equal(contract.ProposalId, dto.ProposalId);
            Assert.Equal(contract.ClientId, dto.ClientId);
            Assert.Equal(contract.FreelancerId, dto.FreelancerId);
            Assert.Equal(contract.AgreedRate, dto.AgreedRate);
            Assert.Equal(contract.Status, dto.Status);
            Assert.Equal(contract.StartedAt, dto.StartedAt);
            Assert.Null(dto.ClosedAt);
        }

        [Fact]
        public void Contract_ToDto_MapsClosedAt_WhenNotNull()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 2,
                ClosedAt = new DateTime(2026, 2, 1)
            };

            // Act
            var dto = contract.ToDto();

            // Assert
            Assert.Equal(new DateTime(2026, 2, 1), dto.ClosedAt);
        }

        [Fact]
        public void WorkDelivery_ToDto_MapsAllFieldsCorrectly()
        {
            // Arrange
            var delivery = new WorkDelivery
            {
                Id = 5,
                ContractId = 1,
                Note = "First draft",
                ActionStatus = ActionStatus.NeedsAttention,
                SubmittedAt = new DateTime(2026, 3, 1)
            };

            // Act
            var dto = delivery.ToDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(delivery.Id, dto.Id);
            Assert.Equal(delivery.ContractId, dto.ContractId);
            Assert.Equal(delivery.Note, dto.Note);
            Assert.Equal(delivery.ActionStatus, dto.ActionStatus);
            Assert.Equal(delivery.SubmittedAt, dto.SubmittedAt);
        }

        [Fact]
        public void DeliveryAttachment_ToDto_MapsAllFieldsCorrectly()
        {
            // Arrange
            var attachmentId = Guid.NewGuid();
            var attachment = new DeliveryAttachment
            {
                Id = attachmentId,
                WorkDeliveryId = 5,
                FileUrl = "https://example.com/file.zip",
                FileType = "application/zip",
                UploadedAt = new DateTime(2026, 4, 1)
            };

            // Act
            var dto = attachment.ToDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(attachment.Id, dto.Id);
            Assert.Equal(attachment.WorkDeliveryId, dto.WorkDeliveryId);
            Assert.Equal(attachment.FileUrl, dto.FileUrl);
            Assert.Equal(attachment.FileType, dto.FileType);
            Assert.Equal(attachment.UploadedAt, dto.UploadedAt);
        }

        [Fact]
        public void ContractReview_ToDto_MapsAllFieldsCorrectly()
        {
            // Arrange
            var review = new ContractReview
            {
                Id = 20,
                ContractId = 1,
                ReviewerId = "client-1",
                Rating = 5,
                Comment = "Great work!",
                CreatedAt = new DateTime(2026, 4, 1)
            };

            // Act
            var dto = review.ToDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(review.Id, dto.Id);
            Assert.Equal(review.ContractId, dto.ContractId);
            Assert.Equal(review.ReviewerId, dto.ReviewerId);
            Assert.Equal(review.Rating, dto.Rating);
            Assert.Equal(review.Comment, dto.Comment);
            Assert.Equal(review.CreatedAt, dto.CreatedAt);
        }
    }
}
