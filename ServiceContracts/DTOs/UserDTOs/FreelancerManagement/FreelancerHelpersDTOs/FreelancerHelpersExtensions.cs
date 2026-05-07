using Entities.Users;
using Entities.Users.FreelancerHelpers;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Linq;

namespace Mappers
{
    public static class FreelancerProfileHelperExtensions
    {
        // =========================================================
        // A. ENTITY TO READ DTO MAPPING (Read Operations)
        // =========================================================

        public static LanguageReadDto ToReadDto(this FreelancerLanguage entity) =>
            new LanguageReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Level = entity.Level
            };

        public static EducationReadDto ToReadDto(this FreelancerEducation entity) =>
            new EducationReadDto
            {
                Id = entity.Id,
                School = entity.School,
                DateStart = entity.DateStart,
                DateEnd = entity.DateEnd,
                Degree = entity.Degree,
                FieldOfStudy = entity.FieldOfStudy
            };

        public static ExperienceDetailReadDto ToReadDto(this FreelancerExperienceDetail entity) =>
            new ExperienceDetailReadDto
            {
                Id = entity.Id,
                Subject = entity.Subject,
                Description = entity.Description
            };

        public static EmploymentReadDto ToReadDto(this FreelancerEmployment entity) =>
            new EmploymentReadDto
            {
                Id = entity.Id,
                Company = entity.Company,
                City = entity.City,
                Country = entity.Country,
                Title = entity.Title,
                CurrentlyWorkThere = entity.CurrentlyWorkThere,
                FromDate = entity.FromDate,
                ToDate = entity.ToDate
            };


        // =========================================================
        // B. CREATE DTO TO ENTITY MAPPING (New Record Creation)
        // =========================================================

        public static FreelancerLanguage ToEntity(this LanguageCreateDto dto, string freelancerId) =>
            new FreelancerLanguage
            {
                FreelancerId = freelancerId,
                Name = dto.Name ?? string.Empty,
                Level = dto.Level ?? string.Empty
            };

        public static FreelancerEducation ToEntity(this EducationCreateDto dto, string freelancerId) =>
            new FreelancerEducation
            {
                FreelancerId = freelancerId,
                School = dto.School ?? string.Empty,
                DateStart = dto.DateStart ?? DateTime.UtcNow,
                DateEnd = dto.DateEnd,
                Degree = dto.Degree ?? string.Empty,
                FieldOfStudy = dto.FieldOfStudy ?? string.Empty
            };

        public static FreelancerExperienceDetail ToEntity(this ExperienceDetailCreateDto dto, string freelancerId) =>
            new FreelancerExperienceDetail
            {
                FreelancerId = freelancerId,
                Subject = dto.Subject ?? string.Empty,
                Description = dto.Description ?? string.Empty
            };

        public static FreelancerEmployment ToEntity(this EmploymentCreateDto dto, string freelancerId) =>
            new FreelancerEmployment
            {
                FreelancerId = freelancerId,
                Company = dto.Company ?? string.Empty,
                City = dto.City ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                Title = dto.Title ?? string.Empty,
                CurrentlyWorkThere = dto.CurrentlyWorkThere ?? false,
                FromDate = dto.FromDate ?? DateTime.UtcNow,
                ToDate = dto.ToDate
            };

        // =========================================================
        // C. UPDATE DTO TO ENTITY MAPPING (Updating Existing Records)
        // =========================================================

        public static FreelancerLanguage ToEntity(this LanguageUpdateDto dto, string freelancerId) =>
            new FreelancerLanguage
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                Name = dto.Name ?? string.Empty,
                Level = dto.Level ?? string.Empty
            };

        public static FreelancerEducation ToEntity(this EducationUpdateDto dto, string freelancerId) =>
            new FreelancerEducation
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                School = dto.School ?? string.Empty,
                DateStart = dto.DateStart ?? DateTime.UtcNow,
                DateEnd = dto.DateEnd,
                Degree = dto.Degree ?? string.Empty,
                FieldOfStudy = dto.FieldOfStudy ?? string.Empty
            };

        public static FreelancerExperienceDetail ToEntity(this ExperienceDetailUpdateDto dto, string freelancerId) =>
            new FreelancerExperienceDetail
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                Subject = dto.Subject ?? string.Empty,
                Description = dto.Description ?? string.Empty
            };

        public static FreelancerEmployment ToEntity(this EmploymentUpdateDto dto, string freelancerId) =>
            new FreelancerEmployment
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                Company = dto.Company ?? string.Empty,
                City = dto.City ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                Title = dto.Title ?? string.Empty,
                CurrentlyWorkThere = dto.CurrentlyWorkThere ?? false,
                FromDate = dto.FromDate ?? DateTime.UtcNow,
                ToDate = dto.ToDate
            };
    }
}
