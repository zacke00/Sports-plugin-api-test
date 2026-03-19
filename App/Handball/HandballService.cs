using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Handball;

public interface IHandballService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncGamesByDateAsync(DateOnly date);
}

public class HandballService(HybridCache cache, SportsVenuesScaffoldContext db, HandballClient client) : IHandballService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesScaffoldContext _db = db;
    private readonly HandballClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "handball-fixtures-all",
            async cancel => await _db.fixtures
                .Where(f => f.Sport_type == "handball")
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
            $"handball-fixture-{id}",
            async cancel => await _db.fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.id == id && f.Sport_type == "handball", cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            },
            cancellationToken: token
        );
    }

    public async Task SyncGamesByDateAsync(DateOnly date)
    {
        // Fetch from external API
        var external = await _client.GetGamesByDateAsync(date);
        if (external?.Response == null) return;

        foreach (var g in external.Response)
        {
            var providerFixtureId = g.Id.ToString();

            var model = new Fixture
            {
                Provider = "api-sports-handball",
                Provider_fixture_id = providerFixtureId,
                Sport_type = "handball",
                League_name = g.League?.Name,
                Starts_at = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                Home_team_name = g.Teams?.Home?.Name,
                Away_team_name = g.Teams?.Away?.Name,
                Home_score = g.Scores?.Home,
                Away_score = g.Scores?.Away,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.Provider == "api-sports-handball" &&
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
        await _cache.RemoveAsync("handball-fixtures-all");
    }
}
