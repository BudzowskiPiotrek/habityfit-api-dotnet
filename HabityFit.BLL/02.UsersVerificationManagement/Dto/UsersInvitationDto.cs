using System.Text.Json.Serialization;

namespace HabityFit.BLL._02.UsersVerificationManagement.Dto
{
    public class UsersInvitationDto
    {
        [JsonPropertyName("ID")]
        public required Guid Id { get; set; }

        [JsonPropertyName("EMAIL")]
        public required string Email { get; set; }
    }
}
