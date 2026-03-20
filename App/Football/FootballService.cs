using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using Sport.App.Data.Entities;

namespace Sport.App.Football;

public interface IFootballService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to);
}

public class FootballService(HybridCache cache, SportsVenuesContext db, FootballClient client) : IFootballService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesContext _db = db;
    private readonly FootballClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "football-fixtures-all",
            async cancel => await _db.Fixtures
                .Where(f => f.SportType == "football")
                .AsNoTracking()
                .ToListAsync(cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            },
            cancellationToken: token
        );
    }

    public async Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            $"football-fixture-{id}",
            async cancel => await _db.Fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && f.SportType == "football", cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            },
            cancellationToken: token
        );
    }

    public async Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to)
    {
        // Delete existing fixtures in the date range only if a range is provided.
        if (from is not null && to is not null)
        {
            var parsedFrom = from.Value.ToDateTime(TimeOnly.MinValue);
            var parsedTo = to.Value.ToDateTime(TimeOnly.MinValue);

            var toDelete = await _db.Fixtures.Where(f => f.StartsAt >= parsedFrom && f.StartsAt <= parsedTo && f.SportType == "football").ToListAsync();
            if (toDelete.Count != 0)
            {
                _db.Fixtures.RemoveRange(toDelete);
                await _db.SaveChangesAsync();
            }
        }

        // Fetch from external API
        var external = await _client.GetFixturesByRangeAsync(league, season, from, to);
        if (external?.Response == null) return;

        foreach (var item in external.Response)
        {
            var fx = item.Fixture;
            if (fx?.Date is null) continue;

            var teams = item.Teams;
            var goals = item.Goals;

            var providerFixtureId = fx.Id.ToString();

            var model = new Fixture
            {
                Provider = "api-sports",
                ProviderFixtureId = providerFixtureId,
                SportType = "football",
                LeagueName = item.League?.Name,
                StartsAt = DateTime.Parse(fx.Date).ToUniversalTime(),
                HomeTeamName = teams?.Home?.Name,
                HomeTeamLogo = teams?.Home?.Logo,
                AwayTeamName = teams?.Away?.Name,
                AwayTeamLogo = teams?.Away?.Logo,
                HomeScore = goals?.Home,
                AwayScore = goals?.Away,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.Fixtures
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f =>
                f.Provider == "api-sports" &&
                f.ProviderFixtureId == providerFixtureId);
            if (exists != null)
            {
                exists.DeletedAt = null; // Restore if previously soft-deleted
                _db.Entry(exists).CurrentValues.SetValues(model);
            }
            else
            {
                await _db.Fixtures.AddAsync(model);
            }
        }

        await _db.SaveChangesAsync();

        // Invalidate cache so next read fetches fresh data
        await _cache.RemoveAsync("football-fixtures-all");
    }
}
