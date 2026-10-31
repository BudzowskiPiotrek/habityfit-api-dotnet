using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._05.MuscleGroupsManagement.Dto
{
    public class MuscleGroupDto
    {
        [JsonPropertyName("ID")]
        public Guid Id { get; set; }

        [JsonPropertyName("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("DELETED_AT")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(ValidationLimits.MuscleGroup.NameMaxLength)]
        [JsonPropertyName("NAME")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(ValidationLimits.MuscleGroup.DescriptionMaxLength)]
        [JsonPropertyName("DESCRIPTION")]
        public string? Description { get; set; }

        [JsonPropertyName("MIN_SETS")]
        public int? MinSets { get; set; }

        [JsonPropertyName("MAX_SETS")]
        public int? MaxSets { get; set; }
    }
}
