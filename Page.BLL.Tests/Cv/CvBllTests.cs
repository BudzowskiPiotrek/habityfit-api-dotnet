using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Page.BLL._2.CvManagement;
using Page.DAL._1.Entity;
using Page.DAL._2.Context;

namespace Page.BLL.Tests.Cv
{
    public class CvBllTests
    {
        private static PageDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PageDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PageDbContext(options);
        }

        #region GetCv
        [Fact]
        public async Task GetCv_WhenProfileExists_ReturnsProfileLinksAndExperiencesOrdered()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.CvProfiles.Add(new CvProfile
            {
                Id = 1,
                FullName = "Jane Doe",
                Role = "Full Stack Developer",
                Summary = "Summary",
                Location = "Madrid",
                Email = "jane@example.com",
                Availability = "Open to work",
                Github = "https://github.com/jane",
                Linkedin = "https://linkedin.com/in/jane",
                CvEs = "https://example.com/cv-es.pdf",
                CvEn = "https://example.com/cv-en.pdf"
            });
            context.CvExperiences.AddRange(
                new CvExperience
                {
                    CompanyName = "Second company",
                    Location = "Remote",
                    EmploymentType = EmploymentType.Contract,
                    Role = "Backend Developer",
                    StartDate = "2024-01",
                    EndDate = "2025-06",
                    InCurrentRole = false,
                    ShortSummary = "Second summary",
                    Skills = ["C#", "SQL"],
                    SortOrder = 2
                },
                new CvExperience
                {
                    CompanyName = "First company",
                    Location = "Barcelona",
                    EmploymentType = EmploymentType.FullTime,
                    Role = "Frontend Developer",
                    StartDate = "2020-01",
                    EndDate = null,
                    InCurrentRole = true,
                    ShortSummary = "First summary",
                    Skills = ["React", "TypeScript"],
                    SortOrder = 1
                });
            await context.SaveChangesAsync();

            var cvBll = new CvBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await cvBll.GetCv();
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().NotBeNull();
            result!.Profile.FullName.Should().Be("Jane Doe");
            result.Profile.Role.Should().Be("Full Stack Developer");
            result.Profile.Summary.Should().Be("Summary");
            result.Profile.Location.Should().Be("Madrid");
            result.Profile.Email.Should().Be("jane@example.com");
            result.Profile.Availability.Should().Be("Open to work");
            result.Profile.Links.Github.Should().Be("https://github.com/jane");
            result.Profile.Links.Linkedin.Should().Be("https://linkedin.com/in/jane");
            result.Profile.Links.CvEs.Should().Be("https://example.com/cv-es.pdf");
            result.Profile.Links.CvEn.Should().Be("https://example.com/cv-en.pdf");
            result.Experience.Should().HaveCount(2);
            result.Experience.Select(e => e.CompanyName).Should().ContainInOrder("First company", "Second company");

            var firstExperience = result.Experience[0];
            firstExperience.Location.Should().Be("Barcelona");
            firstExperience.EmploymentType.Should().Be(EmploymentType.FullTime);
            firstExperience.Role.Should().Be("Frontend Developer");
            firstExperience.StartDate.Should().Be("2020-01");
            firstExperience.EndDate.Should().BeNull();
            firstExperience.InCurrentRole.Should().BeTrue();
            firstExperience.ShortSummary.Should().Be("First summary");
            firstExperience.Skills.Should().BeEquivalentTo(new[] { "React", "TypeScript" });
        }

        [Fact]
        public async Task GetCv_WhenThereAreDeletedExperiences_ExcludesThem()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.CvProfiles.Add(new CvProfile
            {
                Id = 1,
                FullName = "Jane Doe",
                Role = "Developer",
                Summary = "Summary",
                Location = "Madrid",
                Email = "jane@example.com",
                Github = "https://github.com/jane",
                Linkedin = "https://linkedin.com/in/jane",
                CvEs = "https://example.com/cv-es.pdf",
                CvEn = "https://example.com/cv-en.pdf"
            });
            context.CvExperiences.AddRange(
                new CvExperience
                {
                    CompanyName = "Active company",
                    Location = "Madrid",
                    EmploymentType = EmploymentType.FullTime,
                    Role = "Developer",
                    StartDate = "2020-01",
                    InCurrentRole = true,
                    ShortSummary = "Active summary",
                    Skills = ["C#"],
                    SortOrder = 1
                },
                new CvExperience
                {
                    CompanyName = "Deleted company",
                    Location = "Madrid",
                    EmploymentType = EmploymentType.FullTime,
                    Role = "Developer",
                    StartDate = "2018-01",
                    EndDate = "2019-12",
                    InCurrentRole = false,
                    ShortSummary = "Deleted summary",
                    Skills = ["Java"],
                    SortOrder = 2,
                    DeletedAt = DateTime.UtcNow
                });
            await context.SaveChangesAsync();

            var cvBll = new CvBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await cvBll.GetCv();
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().NotBeNull();
            result!.Experience.Should().ContainSingle();
            result.Experience[0].CompanyName.Should().Be("Active company");
        }

        [Fact]
        public async Task GetCv_WhenProfileDoesNotExist_ReturnsNull()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var cvBll = new CvBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await cvBll.GetCv();
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCv_WhenOnlyDeletedProfileExists_ReturnsNull()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.CvProfiles.Add(new CvProfile
            {
                Id = 1,
                FullName = "Jane Doe",
                Role = "Developer",
                Summary = "Summary",
                Location = "Madrid",
                Email = "jane@example.com",
                Github = "https://github.com/jane",
                Linkedin = "https://linkedin.com/in/jane",
                CvEs = "https://example.com/cv-es.pdf",
                CvEn = "https://example.com/cv-en.pdf",
                DeletedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var cvBll = new CvBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await cvBll.GetCv();
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCv_WhenProfileHasNoExperiences_ReturnsEmptyList()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            context.CvProfiles.Add(new CvProfile
            {
                Id = 1,
                FullName = "Jane Doe",
                Role = "Developer",
                Summary = "Summary",
                Location = "Madrid",
                Email = "jane@example.com",
                Github = "https://github.com/jane",
                Linkedin = "https://linkedin.com/in/jane",
                CvEs = "https://example.com/cv-es.pdf",
                CvEn = "https://example.com/cv-en.pdf"
            });
            await context.SaveChangesAsync();

            var cvBll = new CvBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await cvBll.GetCv();
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().NotBeNull();
            result!.Experience.Should().BeEmpty();
        }
        #endregion
    }
}
