using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.FormulaOne;

public interface IFormulaOneService
{
    Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default);
    Task<Fixture?> GetByIdAsync(ulong id, CancellationToken token = default);
    Task SyncFixturesRangeAsync(int season, DateOnly? from, DateOnly? to);
}

public class FormulaOneService(HybridCache cache, SportsVenuesScaffoldContext db, FormulaOneClient client) : IFormulaOneService
{
    private readonly HybridCache _cache = cache;
    private readonly SportsVenuesScaffoldContext _db = db;
    private readonly FormulaOneClient _client = client;

    public async Task<IEnumerable<Fixture>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "formulaone-fixtures-all",
            async cancel => await _db.fixtures
                .Where(f => f.Sport_type == "formula-1")
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
            $"formulaone-fixture-{id}",
            async cancel => await _db.fixtures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.id == id && f.Sport_type == "formula-1", cancel),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            },
            cancellationToken: token
        );
    }

    public async Task SyncFixturesRangeAsync(int season, DateOnly? from, DateOnly? to)
    {
        // Delete existing fixtures in the date range only if a range is provided.
        if (from is not null && to is not null)
        {
            var parsedFrom = from.Value.ToDateTime(TimeOnly.MinValue);
            var parsedTo = to.Value.ToDateTime(TimeOnly.MinValue);

            var toDelete = await _db.fixtures.Where(f => f.Starts_at >= parsedFrom && f.Starts_at <= parsedTo && f.Sport_type == "formula-1").ToListAsync();
            if (toDelete.Count != 0)
            {
                _db.fixtures.RemoveRange(toDelete);
                await _db.SaveChangesAsync();
            }
        }

        // Fetch from external API
        var external = await _client.GetRacesByRangeAsync(season, from, to);
        if (external?.Response == null || external.Response.Count == 0)
        {
            return;
        }

        foreach (var r in external.Response)
        {
            var providerFixtureId = r.Id.ToString();

            var model = new Fixture
            {
                Provider = "api-sports-formula1",
                Provider_fixture_id = providerFixtureId,
                Sport_type = "formula-1",
                Race_name = r.Competition?.Name,
                Starts_at = DateTimeOffset.Parse(r.Date!).UtcDateTime,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.Provider == "api-sports-formula1" &&
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
        await _cache.RemoveAsync("formulaone-fixtures-all");
    }
}
