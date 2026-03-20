using Microsoft.EntityFrameworkCore;

namespace Sport.App.Data;

/// <summary>
/// Background service that periodically runs ANALYZE TABLE and OPTIMIZE TABLE
/// on every table registered in the EF Core model so MySQL keeps index
/// statistics up to date for the query optimizer.
/// </summary>
public class IndexOptimizationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IndexOptimizationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public IndexOptimizationService(
        IServiceProvider serviceProvider,
        ILogger<IndexOptimizationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a short time after startup before the first run so the app
        // can finish initialising (migrations, warm-up, etc.).
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await OptimizeIndexesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Index optimization cycle failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    /// <summary>
    /// Discovers every table name from the EF Core model metadata so new
    /// entities are picked up automatically without maintaining a manual list.
    /// </summary>
    private static IReadOnlyList<string> GetTableNames(SportsVenuesContext db)
    {
        return db.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .Order()
            .ToList()!;
    }

    private async Task OptimizeIndexesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SportsVenuesContext>();

        var database = db.Database.GetDbConnection().Database;
        var tables = GetTableNames(db);

        _logger.LogInformation(
            "Starting index optimization for database {Database} ({TableCount} tables)",
            database, tables.Count);

        foreach (var table in tables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ANALYZE TABLE updates the index statistics that the MySQL
                // query optimizer uses to choose execution plans.
                // It is a non-blocking read-lock on InnoDB and is safe to run
                // while the application is serving traffic.
                var analyzeSql = "ANALYZE TABLE `" + table + "`";
                await db.Database.ExecuteSqlRawAsync(
                    analyzeSql, cancellationToken);

                _logger.LogInformation("ANALYZE TABLE completed for {Table}", table);

                // OPTIMIZE TABLE reclaims unused space and defragments the
                // data file. On InnoDB this triggers an online ALTER TABLE
                // rebuild, so it is heavier — but perfectly safe for moderate-
                // sized tables.
                var optimizeSql = "OPTIMIZE TABLE `" + table + "`";
                await db.Database.ExecuteSqlRawAsync(
                    optimizeSql, cancellationToken);

                _logger.LogInformation("OPTIMIZE TABLE completed for {Table}", table);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to optimize table {Table}", table);
            }
        }

        _logger.LogInformation("Index optimization cycle finished");
    }
}
