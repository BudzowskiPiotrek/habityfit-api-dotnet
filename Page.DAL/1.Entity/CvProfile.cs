using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Page.DAL._1.Entity
{
    [Table("CV_PROFILE")]
    public class CvProfile : BaseEntity
    {
        [Key]
        [Column("ID", Order = 1)]
        public int Id { get; set; }

        [Column("FULL_NAME", Order = 5)]
        public required string FullName { get; set; }

        [Column("ROLE", Order = 6)]
        public required string Role { get; set; }

        [Column("SUMMARY", Order = 7)]
        public required string Summary { get; set; }

        [Column("LOCATION", Order = 8)]
        public required string Location { get; set; }

        [Column("EMAIL", Order = 9)]
        public required string Email { get; set; }

        [Column("AVAILABILITY", Order = 10)]
        public string? Availability { get; set; }

        [Column("GITHUB", Order = 11)]
        public required string Github { get; set; }

        [Column("LINKEDIN", Order = 12)]
        public required string Linkedin { get; set; }

        [Column("CV_ES", Order = 13)]
        public required string CvEs { get; set; }

        [Column("CV_EN", Order = 14)]
        public required string CvEn { get; set; }
    }
}
