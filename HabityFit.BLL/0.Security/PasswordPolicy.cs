using HabityFit.BLL._0.Common;

namespace HabityFit.BLL._1.Security
{
    public static class PasswordPolicy
    {
        public const int MinLength = ValidationLimits.Password.MinLength;
        public const int MaxLength = ValidationLimits.Password.MaxLength;

        private static readonly HashSet<string> CommonPasswords =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "password", "password123", "123456789012", "qwertyuiop12",
                "contraseña123", "admin1234567", "habityfit123"
            };

        public static IReadOnlyList<string> Validate(string? password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("La contraseña es obligatoria.");
                return errors;
            }

            if (password.Length < MinLength)
                errors.Add($"La contraseña debe tener al menos {MinLength} caracteres.");

            if (password.Length > MaxLength)
                errors.Add($"La contraseña no puede superar los {MaxLength} caracteres.");

            if (CommonPasswords.Contains(password))
                errors.Add("La contraseña es demasiado común.");

            return errors;
        }
    }
}
