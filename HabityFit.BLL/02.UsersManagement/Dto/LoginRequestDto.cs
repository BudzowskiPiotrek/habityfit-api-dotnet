using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._02.UsersManagement.Dto
{
    public class LoginRequestDto
    {
        [MaxLength(ValidationLimits.User.EmailMaxLength)]
        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(ValidationLimits.Password.MaxLength)]
        [JsonPropertyName("Password")]
        public string Password { get; set; } = string.Empty;
    }
}
