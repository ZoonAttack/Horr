using Entities.Enums;

namespace ServiceContracts.DTOs.Recommendations
{
    public class TrackInteractionDTO
    {
        public string TargetId { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty; // "job" or "freelancer"
        public InteractionTypes Action { get; set; }
    }
}
