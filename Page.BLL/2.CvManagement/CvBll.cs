using Microsoft.EntityFrameworkCore;
using Page.BLL._2.CvManagement.Dto;
using Page.DAL._2.Context;

namespace Page.BLL._2.CvManagement
{
    public class CvBll : ICvBll
    {
        #region Fields
        private readonly PageDbContext _context;
        #endregion


        #region Constructors
        public CvBll(PageDbContext context)
        {
            _context = context;
        }
        #endregion


        #region Methods
        public async Task<CvContentDto?> GetCv()
        {
            var profile = await _context.CvProfiles.AsNoTracking()
                .Where(p => p.DeletedAt == null)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();

            if (profile is null)
                return null;

            var experiences = await _context.CvExperiences.AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Id)
                .ToListAsync();

            var experience = experiences.Select(e => new CvExperienceDto
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                Location = e.Location,
                EmploymentType = e.EmploymentType,
                Role = e.Role,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                InCurrentRole = e.InCurrentRole,
                ShortSummary = e.ShortSummary,
                Skills = e.Skills.ToList()
            }).ToList();

            return new CvContentDto
            {
                Profile = new CvProfileDto
                {
                    FullName = profile.FullName,
                    Role = profile.Role,
                    Summary = profile.Summary,
                    Location = profile.Location,
                    Email = profile.Email,
                    Availability = profile.Availability,
                    Links = new CvLinksDto
                    {
                        Github = profile.Github,
                        Linkedin = profile.Linkedin,
                        CvEs = profile.CvEs,
                        CvEn = profile.CvEn
                    }
                },
                Experience = experience
            };
        }
        #endregion
    }
}
