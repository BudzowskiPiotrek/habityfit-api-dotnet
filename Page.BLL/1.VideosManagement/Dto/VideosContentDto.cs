namespace Page.BLL._1.VideosManagement.Dto
{
    public class VideosContentDto
    {
        public VideoHeroDto Hero { get; set; } = new();
        public List<VideoItemDto> Videos { get; set; } = [];
    }
}
