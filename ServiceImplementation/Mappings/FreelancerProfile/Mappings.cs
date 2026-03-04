using System;
using System.Collections.Generic;
using System.Linq;
using Entities.Users.FreelancerHelpers;
using ServiceContracts.DTOs.FreelancerProfile;

namespace ServiceImplementation.Mappings.FreelancerProfile
{
    public static class PortfolioMappingExtensions
    {
        public static PortfolioResponseDto ToDto(this PortfolioItem item)
        {
            if (item == null) return null;

            return new PortfolioResponseDto
            {
                Id = item.Id,
                Title = item.Title,
                Role = item.Role,
                Description = item.Description,
                MediaUrl = item.MediaUrl
            };
        }

        public static IEnumerable<PortfolioResponseDto> ToDtoList(this IEnumerable<PortfolioItem> items)
        {
            return items.Select(i => i.ToDto());
        }

        public static PortfolioItem ToEntity(this PortfolioCreateDto dto, Guid userId)
        {
            if (dto == null) return null;

            return new PortfolioItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Role = dto.Role,
                Description = dto.Description,
                MediaUrl = dto.MediaUrl,
                IsDeleted = false
            };
        }
    }

    public static class ExperienceMappingExtensions
    {
        public static ExperienceResponseDto ToDto(this ProfessionalExperience experience)
        {
            if (experience == null) return null;

            string categoryString = string.Empty;

            switch (experience.Category)
            {
                case ExperienceCategory.Employment:
                    categoryString = "Employment History";
                    break;
                case ExperienceCategory.Certification:
                    categoryString = "Certification";
                    break;
                case ExperienceCategory.Other:
                default:
                    categoryString = "Other Experiences";
                    break;
            }

            return new ExperienceResponseDto
            {
                Id = experience.Id,
                Title = experience.Title,
                Category = categoryString
            };
        }

        public static IEnumerable<ExperienceResponseDto> ToDtoList(this IEnumerable<ProfessionalExperience> experiences)
        {
            return experiences.Select(e => e.ToDto());
        }
    }
}
