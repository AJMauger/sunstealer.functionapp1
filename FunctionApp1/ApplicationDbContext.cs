using Microsoft.EntityFrameworkCore;

namespace Sunstealer.FunctionApp1;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<Table1> table1 { get; set; }
}
