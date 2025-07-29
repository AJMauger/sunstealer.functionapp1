using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Sunstealer.FunctionApp1;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        /* ajm: var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddConsole();
        });

        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging(false);*/
    }

    public DbSet<Table1> table1 { get; set; }
}
