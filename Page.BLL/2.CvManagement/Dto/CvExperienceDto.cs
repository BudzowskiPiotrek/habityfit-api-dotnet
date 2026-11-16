using Page.DAL._1.Entity;

namespace Page.BLL._2.CvManagement.Dto
{
    public class CvExperienceDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; }
        public string Role { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public bool InCurrentRole { get; set; }
        public string ShortSummary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = [];
    }
}
