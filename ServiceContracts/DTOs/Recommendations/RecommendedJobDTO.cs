using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.Recommendations
{
    public class RecommendedJobDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public List<string> Skills { get; set; } = new();
        public bool IsSaved { get; set; }
        public bool IsFallback { get; set; }
    }
}
