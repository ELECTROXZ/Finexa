using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Finexa.Infrastructure.Database
{
    public class FinexaDbContextFactory : IDesignTimeDbContextFactory<FinexaDbContext>
    {
        public FinexaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FinexaDbContext>();
            optionsBuilder.UseSqlite("Data Source=designTime.db");

            return new FinexaDbContext(optionsBuilder.Options);
        }
    }
}
