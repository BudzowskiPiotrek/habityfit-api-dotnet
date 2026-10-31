using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._02.UsersVerificationManagement.Dto
{
    public class UsersInvitationDto
    {
        [JsonPropertyName("ID")]
        public required Guid Id { get; set; }

        [MaxLength(ValidationLimits.User.EmailMaxLength)]
        [JsonPropertyName("EMAIL")]
        public required string Email { get; set; }
    }
}
