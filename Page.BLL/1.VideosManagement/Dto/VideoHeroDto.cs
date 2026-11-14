namespace Page.BLL._1.VideosManagement.Dto
{
    public class VideoHeroDto
    {
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VideoLinksDto Links { get; set; } = new();
    }
}
