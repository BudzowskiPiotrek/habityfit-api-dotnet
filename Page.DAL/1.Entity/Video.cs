using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Page.DAL._1.Entity
{
    public enum SectionType
    {
        NextJs = 1,
        DotNet = 2,
        Docker = 3
    }

    [Table("VIDEO")]
    public class Video : BaseEntity
    {
        [Key]
        [Column("ID", Order = 1)]
        public required string Id { get; set; }

        [Column("TITLE", Order = 5)]
        public required string Title { get; set; }

        [Column("DESCRIPTION", Order = 6)]
        public required string Description { get; set; }

        [Column("DURATION", Order = 7)]
        public required string Duration { get; set; }

        [Column("PUBLISHED_AT", Order = 8)]
        public DateOnly PublishedAt { get; set; }

        [Column("SECTION_TYPE", Order = 9)]
        public SectionType SectionType { get; set; }
    }
}
