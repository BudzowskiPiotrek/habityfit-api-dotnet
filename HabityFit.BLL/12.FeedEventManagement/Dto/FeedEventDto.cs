using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._12.FeedEventManagement.Dto
{
    public class FeedEventDto
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
        public Guid UserId { get; set; } = Guid.Empty;

        [JsonPropertyName("EVENT_TYPE")]
        public FeedEventType EventType { get; set; }

        [JsonPropertyName("EXERCISE_ID")]
        public Guid? ExerciseId { get; set; } = null;

        [JsonPropertyName("METRIC_VALUE")]
        public double? MetricValue { get; set; } = null;

        [MaxLength(ValidationLimits.Feed.CustomTextMaxLength)]
        [JsonPropertyName("CUSTOM_TEXT")]
        public string? CustomText { get; set; } = null;
    }
}
