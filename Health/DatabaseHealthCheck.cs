using Binder_Cart.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Binder_Cart.Health
{
    public class DatabaseHealthCheck : IHealthCheck 
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            var database = _context.Database;
            await database.ExecuteSqlInterpolatedAsync($"select 1", cancellationToken);

            if (await database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Healthy("Database is operating normally.");

            return HealthCheckResult.Unhealthy("Cannot connect to database");
        }
    }
}
