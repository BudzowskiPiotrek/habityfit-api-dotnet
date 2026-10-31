using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._02.UsersVerificationManagement.Dto
{
    public class AcceptInvitationDto
    {
        [JsonPropertyName("TOKEN")]
        public required Guid Token { get; set; }

        [StringLength(ValidationLimits.User.FullNameMaxLength, MinimumLength = ValidationLimits.User.FullNameMinLength)]
        [JsonPropertyName("FULL_NAME")]
        public required string FullName { get; set; }

        [JsonPropertyName("PASSWORD")]
        public required string Password { get; set; }
    }
}
