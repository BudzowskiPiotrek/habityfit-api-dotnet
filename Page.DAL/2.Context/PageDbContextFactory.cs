using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Page.DAL._2.Context
{
    public class PageDbContextFactory : IDesignTimeDbContextFactory<PageDbContext>
    {
        public PageDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PageDbContext>();

            optionsBuilder.UseNpgsql("Host=;Port=;Database=;Username=;Password=");

            return new PageDbContext(optionsBuilder.Options);
        }
    }
}
