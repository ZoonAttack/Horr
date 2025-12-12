using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.User.Freelancer
{
    // =========================================================
    // 1. LANGUAGE DTOs
    // =========================================================

    public class LanguageReadDto
    {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class LanguageCreateDto
    {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class LanguageUpdateDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
    }

    // =========================================================
    // 2. EDUCATION DTOs
    // =========================================================

    public class EducationReadDto
    {
        public string School { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
    }

    public class EducationCreateDto
    {
        public string School { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
    }

    public class EducationUpdateDto
    {
        public int? Id { get; set; }
        public string School { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
    }

    // =========================================================
    // 3. EXPERIENCE DETAIL DTOs
    // =========================================================

    public class ExperienceDetailReadDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    public class ExperienceDetailCreateDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    public class ExperienceDetailUpdateDto
    {
        public int? Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    // =========================================================
    // 4. EMPLOYMENT DTOs
    // =========================================================

    public class EmploymentReadDto
    {
        public string Company { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Title { get; set; }
        public bool CurrentlyWorkThere { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class EmploymentCreateDto
    {
        public string Company { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Title { get; set; }
        public bool CurrentlyWorkThere { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
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
}