using System.Text.Json.Serialization;

namespace HabityFit.BLL._02.UsersVerificationManagement.Dto
{
    public class EmailDto
    {
        [JsonPropertyName("EMAIL")]
        public required string Email { get; set; }
    }
}
