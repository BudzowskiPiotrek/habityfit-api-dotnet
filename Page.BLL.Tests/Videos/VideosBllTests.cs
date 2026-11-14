using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Page.BLL._1.VideosManagement;
using Page.DAL._1.Entity;
using Page.DAL._2.Context;

namespace Page.BLL.Tests.Videos
{
    public class VideosBllTests
    {
        private static PageDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PageDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PageDbContext(options);
        }

        #region GetVideos
        [Fact]
        public async Task GetVideos_WhenSectionExists_ReturnsHeroLinksAndOnlySectionVideos()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.VideoHeroes.Add(new VideoHero
            {
                SectionType = SectionType.NextJs,
                Eyebrow = "Eyebrow",
                Title = "Hero title",
                Description = "Hero description",
                Github = "https://github.com/example",
                Youtube = "https://youtube.com/example"
            });
            context.Videos.AddRange(
                new Video
                {
                    Id = "video-1",
                    Title = "Older",
                    Description = "First video",
                    Duration = "10:00",
                    PublishedAt = new DateOnly(2026, 1, 1),
                    SectionType = SectionType.NextJs
                },
                new Video
                {
                    Id = "video-2",
                    Title = "Newer",
                    Description = "Second video",
                    Duration = "20:00",
                    PublishedAt = new DateOnly(2026, 2, 1),
                    SectionType = SectionType.NextJs
                },
                new Video
                {
                    Id = "video-3",
                    Title = "Other section",
                    Description = "DotNet video",
                    Duration = "30:00",
                    PublishedAt = new DateOnly(2026, 3, 1),
                    SectionType = SectionType.DotNet
                },
                new Video
                {
                    Id = "video-4",
                    Title = "Deleted",
                    Description = "Deleted video",
                    Duration = "40:00",
                    PublishedAt = new DateOnly(2026, 4, 1),
                    SectionType = SectionType.NextJs,
                    DeletedAt = DateTime.UtcNow
                });
            await context.SaveChangesAsync();

            var videosBll = new VideosBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await videosBll.GetVideos(SectionType.NextJs);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().NotBeNull();
            result!.Hero.Eyebrow.Should().Be("Eyebrow");
            result.Hero.Title.Should().Be("Hero title");
            result.Hero.Description.Should().Be("Hero description");
            result.Hero.Links.Github.Should().Be("https://github.com/example");
            result.Hero.Links.Youtube.Should().Be("https://youtube.com/example");
            result.Videos.Should().HaveCount(2);
            result.Videos.Select(v => v.Id).Should().ContainInOrder("video-2", "video-1");
            result.Videos.Should().OnlyContain(v => v.SectionType == SectionType.NextJs);
        }

        [Fact]
        public async Task GetVideos_WhenHeroDoesNotExist_ReturnsNull()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.Videos.Add(new Video
            {
                Id = "video-1",
                Title = "Orphan",
                Description = "Video without hero",
                Duration = "10:00",
                PublishedAt = new DateOnly(2026, 1, 1),
                SectionType = SectionType.NextJs
            });
            await context.SaveChangesAsync();

            var videosBll = new VideosBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await videosBll.GetVideos(SectionType.NextJs);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetVideos_WhenSectionHasNoVideos_ReturnsEmptyList()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.VideoHeroes.Add(new VideoHero
            {
                SectionType = SectionType.NextJs,
                Eyebrow = "Eyebrow",
                Title = "Hero title",
                Description = "Hero description",
                Github = "https://github.com/example",
                Youtube = "https://youtube.com/example"
            });
            await context.SaveChangesAsync();

            var videosBll = new VideosBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await videosBll.GetVideos(SectionType.NextJs);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().NotBeNull();
            result!.Videos.Should().BeEmpty();
        }
        #endregion
    }
}
