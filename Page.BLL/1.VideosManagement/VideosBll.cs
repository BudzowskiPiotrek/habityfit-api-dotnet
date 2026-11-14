using Microsoft.EntityFrameworkCore;
using Page.BLL._1.VideosManagement.Dto;
using Page.DAL._1.Entity;
using Page.DAL._2.Context;

namespace Page.BLL._1.VideosManagement
{
    public class VideosBll : IVideosBll
    {
        #region Fields
        private readonly PageDbContext _context;
        #endregion


        #region Constructors
        public VideosBll(PageDbContext context)
        {
            _context = context;
        }
        #endregion


        #region Methods
        public async Task<VideosContentDto?> GetVideos(SectionType section)
        {
            var hero = await _context.VideoHeroes.AsNoTracking().FirstOrDefaultAsync(h => h.SectionType == section);

            if (hero is null)
                return null;

            var videos = await _context.Videos.AsNoTracking().Where(v => v.SectionType == section && v.DeletedAt == null)
                .OrderByDescending(v => v.PublishedAt)
                .ThenBy(v => v.Title)
                .Select(v => new VideoItemDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    Duration = v.Duration,
                    PublishedAt = v.PublishedAt,
                    SectionType = v.SectionType
                })
                .ToListAsync();

            return new VideosContentDto
            {
                Hero = new VideoHeroDto
                {
                    Eyebrow = hero.Eyebrow,
                    Title = hero.Title,
                    Description = hero.Description,
                    Links = new VideoLinksDto
                    {
                        Github = hero.Github,
                        Youtube = hero.Youtube
                    }
                },
                Videos = videos
            };
        }
        #endregion
    }
}
