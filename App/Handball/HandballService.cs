using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using Sport.App.Data.Entities;

namespace Sport.App.Handball;

public interface IHandballService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncGamesByDateAsync(DateOnly date);
}

public class HandballService(HybridCache cache, SportsVenuesContext db, HandballClient client) : IHandballService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesContext _db = db;
    private readonly HandballClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "handball-fixtures-all",
            async cancel => await _db.Fixtures
                .Where(f => f.SportType == "handball")
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
            async cancel => await _db.Fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && f.SportType == "handball", cancel),
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
                ProviderFixtureId = providerFixtureId,
                SportType = "handball",
                LeagueName = g.League?.Name,
                StartsAt = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                HomeTeamName = g.Teams?.Home?.Name,
                AwayTeamName = g.Teams?.Away?.Name,
                HomeScore = g.Scores?.Home,
                AwayScore = g.Scores?.Away,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.Fixtures
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f =>
                f.Provider == "api-sports-handball" &&
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
        await _cache.RemoveAsync("handball-fixtures-all");
    }
}
