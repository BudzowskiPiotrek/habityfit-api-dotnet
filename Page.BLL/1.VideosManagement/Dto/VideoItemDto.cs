using Page.DAL._1.Entity;

namespace Page.BLL._1.VideosManagement.Dto
{
    public class VideoItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateOnly PublishedAt { get; set; }
        public SectionType SectionType { get; set; }
    }
}
