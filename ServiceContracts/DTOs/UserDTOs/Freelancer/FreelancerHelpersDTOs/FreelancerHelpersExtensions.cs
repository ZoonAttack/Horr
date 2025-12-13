using Entities.Users;
using Entities.Users.FreelancerHelpers;
using ServiceContracts.DTOs.User.Freelancer;
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
                Name = entity.Name,
                Level = entity.Level
            };

        public static EducationReadDto ToReadDto(this FreelancerEducation entity) =>
            new EducationReadDto
            {
                School = entity.School,
                DateStart = entity.DateStart,
                DateEnd = entity.DateEnd,
                Degree = entity.Degree,
                FieldOfStudy = entity.FieldOfStudy
            };

        public static ExperienceDetailReadDto ToReadDto(this FreelancerExperienceDetail entity) =>
            new ExperienceDetailReadDto
            {
                Subject = entity.Subject,
                Description = entity.Description
            };

        public static EmploymentReadDto ToReadDto(this FreelancerEmployment entity) =>
            new EmploymentReadDto
            {
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
                Name = dto.Name,
                Level = dto.Level
            };

        public static FreelancerEducation ToEntity(this EducationCreateDto dto, string freelancerId) =>
            new FreelancerEducation
            {
                FreelancerId = freelancerId,
                School = dto.School,
                DateStart = dto.DateStart,
                DateEnd = dto.DateEnd,
                Degree = dto.Degree,
                FieldOfStudy = dto.FieldOfStudy
            };

        public static FreelancerExperienceDetail ToEntity(this ExperienceDetailCreateDto dto, string freelancerId) =>
            new FreelancerExperienceDetail
            {
                FreelancerId = freelancerId,
                Subject = dto.Subject,
                Description = dto.Description
            };

        public static FreelancerEmployment ToEntity(this EmploymentCreateDto dto, string freelancerId) =>
            new FreelancerEmployment
            {
                FreelancerId = freelancerId,
                Company = dto.Company,
                City = dto.City,
                Country = dto.Country,
                Title = dto.Title,
                CurrentlyWorkThere = dto.CurrentlyWorkThere,
                FromDate = dto.FromDate,
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
                Name = dto.Name,
                Level = dto.Level
            };

        public static FreelancerEducation ToEntity(this EducationUpdateDto dto, string freelancerId) =>
            new FreelancerEducation
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                School = dto.School,
                DateStart = dto.DateStart,
                DateEnd = dto.DateEnd,
                Degree = dto.Degree,
                FieldOfStudy = dto.FieldOfStudy
            };

        public static FreelancerExperienceDetail ToEntity(this ExperienceDetailUpdateDto dto, string freelancerId) =>
            new FreelancerExperienceDetail
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                Subject = dto.Subject,
                Description = dto.Description
            };

        public static FreelancerEmployment ToEntity(this EmploymentUpdateDto dto, string freelancerId) =>
            new FreelancerEmployment
            {
                Id = dto.Id ?? 0,
                FreelancerId = freelancerId,
                Company = dto.Company,
                City = dto.City,
                Country = dto.Country,
                Title = dto.Title,
                CurrentlyWorkThere = dto.CurrentlyWorkThere,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            };
    }
}
public class EmploymentUpdateDto
{
        public int? Id { get; set; }
        public string Company { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Title { get; set; }
        public bool CurrentlyWorkThere { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
}