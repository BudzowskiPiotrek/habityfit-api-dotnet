using Microsoft.EntityFrameworkCore;
using Page.DAL._1.Entity;

namespace Page.DAL._2.Context
{
    public class PageDbContext : DbContext
    {
        #region Constructors
        // --------------------------------------------------------------------------------------------------- 
        public PageDbContext(DbContextOptions<PageDbContext> options) : base(options) { }
        // --------------------------------------------------------------------------------------------------- 
        #endregion


        #region DbSets
        // --------------------------------------------------------------------------------------------------- 
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoHero> VideoHeroes { get; set; }
        public DbSet<CvProfile> CvProfiles { get; set; }
        public DbSet<CvExperience> CvExperiences { get; set; }
        // --------------------------------------------------------------------------------------------------- 
        #endregion


        #region Configuration Indexes
        // --------------------------------------------------------------------------------------------------- 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VideoHero>()
                .Property(h => h.SectionType)
                .ValueGeneratedNever();

            modelBuilder.Entity<CvProfile>()
                .Property(p => p.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Video>()
                .Property(v => v.CreatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<CvProfile>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<CvExperience>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Video>()
                .HasOne<VideoHero>()
                .WithMany()
                .HasForeignKey(v => v.SectionType)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Video>()
                .HasIndex(v => new { v.SectionType, v.PublishedAt });

            modelBuilder.Entity<CvExperience>()
                .HasIndex(e => e.SortOrder);
        }
        // --------------------------------------------------------------------------------------------------- 
        #endregion
    }
}
