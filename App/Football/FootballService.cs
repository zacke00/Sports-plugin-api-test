using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Football;

public interface IFootballService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to);
}

public class FootballService(HybridCache cache, SportsVenuesScaffoldContext db, FootballClient client) : IFootballService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesScaffoldContext _db = db;
    private readonly FootballClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "football-fixtures-all",
            async cancel => await _db.fixtures
                .Where(f => f.Sport_type == "football")
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
            async cancel => await _db.fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.id == id && f.Sport_type == "football", cancel),
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

            var toDelete = await _db.fixtures.Where(f => f.Starts_at >= parsedFrom && f.Starts_at <= parsedTo && f.Sport_type == "football").ToListAsync();
            if (toDelete.Count != 0)
            {
                _db.fixtures.RemoveRange(toDelete);
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
                Provider_fixture_id = providerFixtureId,
                Sport_type = "football",
                League_name = item.League?.Name,
                Starts_at = DateTime.Parse(fx.Date).ToUniversalTime(),
                Home_team_name = teams?.Home?.Name,
                Away_team_name = teams?.Away?.Name,
                Home_score = goals?.Home,
                Away_score = goals?.Away,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.Provider == "api-sports" &&
                f.Provider_fixture_id == providerFixtureId);
            if (exists != null)
            {
                _db.Entry(exists).CurrentValues.SetValues(model);
            }
            else
            {
                await _db.fixtures.AddAsync(model);
            }
        }

        await _db.SaveChangesAsync();

        // Invalidate cache so next read fetches fresh data
        await _cache.RemoveAsync("football-fixtures-all");
    }
}
