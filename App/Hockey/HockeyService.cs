using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Data.Entities;
using Microsoft.Extensions.Caching.Hybrid;

namespace Sport.App.Hockey;

public interface IHockeyService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to);
}

public class HockeyService(HybridCache cache, SportsVenuesContext db, HockeyClient client) : IHockeyService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesContext _db = db;
    private readonly HockeyClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "hockey-fixtures-all",
            async cancel => await _db.Fixtures
                .Where(f => f.SportType == "hockey")
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
            $"hockey-fixture-{id}",
            async cancel => await _db.Fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && f.SportType == "hockey", cancel),
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

            var toDelete = await _db.Fixtures.Where(f => f.StartsAt >= parsedFrom && f.StartsAt <= parsedTo && f.SportType == "hockey").ToListAsync();
            if (toDelete.Count != 0)
            {
                _db.Fixtures.RemoveRange(toDelete);
                await _db.SaveChangesAsync();
            }
        }

        // Fetch from external API
        var external = await _client.GetGamesAsync(league, season, from, to);
        if (external?.Response == null) return;

        foreach (var g in external.Response)
        {
            var providerFixtureId = g.Id.ToString();

            var model = new Fixture
            {
                Provider = "api-sports-hockey",
                ProviderFixtureId = providerFixtureId,
                SportType = "hockey",
                LeagueName = g.League?.Name,
                StartsAt = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                HomeTeamName = g.Teams?.Home?.Name,
                AwayTeamName = g.Teams?.Away?.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Upsert: if exists, update; else add
            var exists = await _db.Fixtures
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f =>
                f.Provider == "api-sports-hockey" &&
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
        await _cache.RemoveAsync("hockey-fixtures-all");
    }
}