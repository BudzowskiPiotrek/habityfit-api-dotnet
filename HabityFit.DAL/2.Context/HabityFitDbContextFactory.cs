using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HabityFit.DAL._2.Context
{
    public class HabityFitDbContextFactory : IDesignTimeDbContextFactory<HabityFitDbContext>
    {
        public HabityFitDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HabityFitDbContext>();

            optionsBuilder.UseNpgsql("Host=;Port=;Database=;Username=;Password=");

            return new HabityFitDbContext(optionsBuilder.Options);
        }
    }
}
