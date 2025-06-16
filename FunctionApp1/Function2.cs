using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sunstealer.FunctionApp1
{
    public class Function2
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicationService? _application;
        private readonly IConfiguration? _configuration;
        private readonly IDbContextFactory<ApplicationDbContext>? _dbContextFactory;
        private readonly ILogger? _logger;
        // ajm: private readonly ILoggerService? _logger;
        private readonly ITelemetryService? _telemetryService;

        public Function2(IApplicationService application, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILoggerFactory loggerFactory, ILoggerService loggerService, ITelemetryService telemetryService)
        {
            _application = application ?? throw new Exception("application");
            _configuration = configuration ?? throw new Exception("configuration");
            _dbContextFactory = dbContextFactory ?? throw new Exception("dbContextFactory");
            _logger = loggerFactory.CreateLogger<Function2>();
            _dbContext = this._dbContextFactory.CreateDbContext();
        }

        /* [Function("Function2")]
        public void Run([SqlTrigger("[dbo].[table1]", "DatabaseConnectionString")] IReadOnlyList<SqlChange<Table1>> changes, FunctionContext context)
        {
                _logger?.LogInformation("SQL Changes: " + JsonConvert.SerializeObject(changes));
        }*/
    }
}
