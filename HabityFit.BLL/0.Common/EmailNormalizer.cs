namespace HabityFit.BLL._0.Common
{
    public static class EmailNormalizer
    {
        public static string Normalize(string email) =>
            email.Trim().ToLowerInvariant();
    }
}
