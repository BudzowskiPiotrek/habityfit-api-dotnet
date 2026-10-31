using FluentAssertions;
using HabityFit.BLL._1.Security;

namespace HabityFit.BLL.Tests.Management
{
    public class PasswordPolicyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Corta1!")]
        [InlineData("12345678901")]
        public void Validate_WhenPasswordIsEmptyOrTooShort_ReturnsErrors(string? password)
        {
            PasswordPolicy.Validate(password).Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_WhenPasswordExceedsMaxLength_ReturnsError()
        {
            var password = new string('a', PasswordPolicy.MaxLength + 1);

            PasswordPolicy.Validate(password).Should().ContainSingle().Which.Should().Contain("128");
        }

        [Theory]
        [InlineData("habityfit123")]
        [InlineData("contraseña123")]
        public void Validate_WhenPasswordIsCommon_ReturnsError(string password)
        {
            PasswordPolicy.Validate(password).Should().ContainSingle().Which.Should().Contain("común");
        }

        [Theory]
        [InlineData("HabityFit-Segura-2026!")]
        [InlineData("unaClaveLargaYPropia")]
        public void Validate_WhenPasswordMeetsPolicy_ReturnsNoErrors(string password)
        {
            PasswordPolicy.Validate(password).Should().BeEmpty();
        }
    }
}
