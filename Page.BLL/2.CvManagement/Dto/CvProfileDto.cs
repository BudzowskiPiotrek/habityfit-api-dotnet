namespace Page.BLL._2.CvManagement.Dto
{
    public class CvProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Availability { get; set; }
        public CvLinksDto Links { get; set; } = new();
    }
}
