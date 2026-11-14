using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Page.DAL._1.Entity
{
    public enum EmploymentType
    {
        FullTime = 1,
        PartTime = 2,
        Contract = 3,
        Internship = 4,
        Freelance = 5
    }

    [Table("CV_EXPERIENCE")]
    public class CvExperience : BaseEntity
    {
        [Key]
        [Column("ID", Order = 1)]
        public int Id { get; set; }

        [Column("COMPANY_NAME", Order = 5)]
        public required string CompanyName { get; set; }

        [Column("LOCATION", Order = 6)]
        public required string Location { get; set; }

        [Column("EMPLOYMENT_TYPE", Order = 7)]
        public EmploymentType EmploymentType { get; set; }

        [Column("ROLE", Order = 8)]
        public required string Role { get; set; }

        [Column("START_DATE", Order = 9)]
        public required string StartDate { get; set; }

        [Column("END_DATE", Order = 10)]
        public string? EndDate { get; set; }

        [Column("IN_CURRENT_ROLE", Order = 11)]
        public bool InCurrentRole { get; set; }

        [Column("SHORT_SUMMARY", Order = 12)]
        public required string ShortSummary { get; set; }

        [Column("SKILLS", Order = 13)]
        public string[] Skills { get; set; } = [];

        [Column("SORT_ORDER", Order = 14)]
        public int SortOrder { get; set; }
    }
}
