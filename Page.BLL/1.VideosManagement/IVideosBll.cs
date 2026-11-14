using Page.BLL._1.VideosManagement.Dto;
using Page.DAL._1.Entity;

namespace Page.BLL._1.VideosManagement
{
    public interface IVideosBll
    {
        Task<VideosContentDto?> GetVideos(SectionType section);
    }
}
