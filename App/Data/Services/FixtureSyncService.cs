using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sport.App.Data;
using Sport.App.Football;
using Sport.App.FormulaOne;
using Sport.App.Handball;
using Sport.App.Hockey;

namespace Sport.App.Data.Services;

/// <summary>
/// Background service that periodically syncs fixtures from every configured
/// sport provider.  Runs once shortly after startup and then repeats on a
/// fixed interval.
/// </summary>
public class FixtureSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FixtureSyncService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public FixtureSyncService(
        IServiceProvider serviceProvider,
        ILogger<FixtureSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish starting (migrations, warm-up, etc.)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Fixture sync cycle starting");

            try
            {
                await RunSyncCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Fixture sync cycle failed");
            }

            _logger.LogInformation("Fixture sync cycle finished — next run in {Interval}", _interval);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunSyncCycleAsync(CancellationToken ct)
    {
        // Remove fixtures whose start time is more than 24 hours ago
        await PurgeExpiredFixturesAsync(ct);

        var currentYear = DateTime.UtcNow.Year;
        var fallbackSeasons = new[] { currentYear, currentYear - 1 };

        await SyncFootballAsync(fallbackSeasons, ct);
        await SyncHockeyAsync(fallbackSeasons, ct);
        await SyncFormulaOneAsync(fallbackSeasons, ct);
        await SyncHandballAsync(ct);
    }

    // ── Purge ───────────────────────────────────────────────────────────
    /// <summary>
    /// Hard-deletes every fixture whose <c>StartsAt</c> is more than 24 hours
    /// in the past.  The <c>venue_fixtures</c> join rows are cascade-deleted
    /// by the database.
    /// </summary>
    private async Task PurgeExpiredFixturesAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SportsVenuesContext>();

            var cutoff = DateTime.UtcNow.AddHours(-24);

            // IgnoreQueryFilters so we also catch any soft-deleted rows
            var purged = await db.Fixtures
                .IgnoreQueryFilters()
                .Where(f => f.StartsAt < cutoff)
                .ExecuteDeleteAsync(ct);

            if (purged > 0)
                _logger.LogInformation("Purged {Count} expired fixture(s) older than {Cutoff:u}", purged, cutoff);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Fixture purge failed");
        }
    }

    // ── Football ────────────────────────────────────────────────────────
    private async Task SyncFootballAsync(int[] seasons, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IFootballService>();
        var opts = scope.ServiceProvider.GetRequiredService<IOptions<FootballOptions>>().Value;

        if (opts.Leagues.Length == 0)
        {
            _logger.LogWarning("Football sync skipped — no leagues configured");
            return;
        }

        foreach (var season in seasons)
        {
            foreach (var league in opts.Leagues)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    _logger.LogInformation(
                        "Syncing Football fixtures: league={League}, season={Season}",
                        league, season);

                    await svc.SyncFixturesRangeAsync(league, season, from: null, to: null);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "Football sync failed for league={League}, season={Season}",
                        league, season);
                }
            }
        }
    }

    // ── Hockey ──────────────────────────────────────────────────────────
    private async Task SyncHockeyAsync(int[] fallbackSeasons, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IHockeyService>();
        var opts = scope.ServiceProvider.GetRequiredService<IOptions<HockeyOptions>>().Value;

        if (opts.Leagues.Length == 0)
        {
            _logger.LogWarning("Hockey sync skipped — no leagues configured");
            return;
        }

        var seasons = opts.Seasons.Length > 0 ? opts.Seasons : fallbackSeasons;

        foreach (var season in seasons)
        {
            foreach (var league in opts.Leagues)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    _logger.LogInformation(
                        "Syncing Hockey fixtures: league={League}, season={Season}",
                        league, season);

                    await svc.SyncFixturesRangeAsync(league, season, from: null, to: null);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "Hockey sync failed for league={League}, season={Season}",
                        league, season);
                }
            }
        }
    }

    // ── Formula One ─────────────────────────────────────────────────────
    private async Task SyncFormulaOneAsync(int[] fallbackSeasons, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IFormulaOneService>();
        var opts = scope.ServiceProvider.GetRequiredService<IOptions<FormulaOneOptions>>().Value;

        var seasons = opts.Seasons.Length > 0 ? opts.Seasons : fallbackSeasons;

        foreach (var season in seasons)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                _logger.LogInformation(
                    "Syncing Formula One fixtures: season={Season}", season);

                await svc.SyncFixturesRangeAsync(season, from: null, to: null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Formula One sync failed for season={Season}", season);
            }
        }
    }

    // ── Handball ─────────────────────────────────────────────────────────
    // Handball's API syncs by date. We sync today plus every day up to
    // DaysAhead (configurable, default 8) days in the future.
    private async Task SyncHandballAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IHandballService>();
        var opts = scope.ServiceProvider.GetRequiredService<IOptions<HandballOptions>>().Value;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysAhead = Math.Max(0, opts.DaysAhead);

        for (var offset = 0; offset <= daysAhead; offset++)
        {
            ct.ThrowIfCancellationRequested();

            var date = today.AddDays(offset);
            try
            {
                _logger.LogInformation(
                    "Syncing Handball fixtures: date={Date}", date);

                await svc.SyncGamesByDateAsync(date);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Handball sync failed for date={Date}", date);
            }
        }
    }
}
