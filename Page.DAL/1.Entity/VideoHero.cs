using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Page.DAL._1.Entity
{
    [Table("VIDEO_HERO")]
    public class VideoHero
    {
        [Key]
        [Column("SECTION_TYPE", Order = 1)]
        public SectionType SectionType { get; set; }

        [Column("EYEBROW", Order = 2)]
        public required string Eyebrow { get; set; }

        [Column("TITLE", Order = 3)]
        public required string Title { get; set; }

        [Column("DESCRIPTION", Order = 4)]
        public required string Description { get; set; }

        [Column("GITHUB", Order = 5)]
        public required string Github { get; set; }

        [Column("YOUTUBE", Order = 6)]
        public required string Youtube { get; set; }
    }
}
