using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogiMarket.Infrastructure.Data;

public class LogiMarketDbContextFactory : IDesignTimeDbContextFactory<LogiMarketDbContext>
{
    public LogiMarketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LogiMarketDbContext>();
        optionsBuilder.UseSqlite("Data Source=LogiMarket.db");

        return new LogiMarketDbContext(optionsBuilder.Options);
    }
}
