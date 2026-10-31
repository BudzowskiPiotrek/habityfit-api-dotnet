using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._09.WorkoutSessionsManagement.Dto
{
    public class WorkoutSessionsDto
    {
        [JsonPropertyName("ID")]
        public Guid Id { get; set; }

        [JsonPropertyName("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("DELETED_AT")]
        public DateTime? DeletedAt { get; set; }

        [JsonPropertyName("USER_ID")]
        public Guid UserId { get; set; }

        [JsonPropertyName("DATE")]
        public DateTime Date { get; set; }

        [JsonPropertyName("DURATION")]
        public int DurationMinutes { get; set; }

        [MaxLength(ValidationLimits.WorkoutSession.NotesMaxLength)]
        [JsonPropertyName("NOTES")]
        public string? Notes { get; set; }
    }
}
