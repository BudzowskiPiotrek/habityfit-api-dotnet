using HabityFit.BLL._0.Common;
using HabityFit.DAL._1.Entity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HabityFit.BLL._02.UsersManagement.Dto
{
    public class UserReadDto
    {
        [JsonPropertyName("ID")]
        public Guid? Id { get; set; }

        [JsonPropertyName("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("DELETED_AT")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(ValidationLimits.User.EmailMaxLength)]
        [JsonPropertyName("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(ValidationLimits.User.FullNameMaxLength)]
        [JsonPropertyName("NAME")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("SEX")]
        public Sex? Sex { get; set; }

        [JsonPropertyName("BIRTH_DATE")]
        public DateTime? BirthDate { get; set; }

        [JsonPropertyName("HEIGHT")]
        public double? Height { get; set; }

        [JsonPropertyName("TERMS_ACCEPTED_AT")]
        public DateTime? TermsAcceptedAt { get; set; }
    }
}
