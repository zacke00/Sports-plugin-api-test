using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Sport.App.Data;
using VenueFixtureEntity = Sport.App.Data.Entities.VenueFixture;

namespace Sport.App.VenueFixture;

public record VenueFixtureDto(
    ulong VenueId,
    ulong FixtureId,
    DateTime CreatedAt
);

public interface IFixtureListService
{
    Task<IEnumerable<VenueFixtureDto>> GetAllAsync(CancellationToken token = default);
    Task<IEnumerable<VenueFixtureDto>> GetByVenueAsync(ulong venueId);
    Task AddAsync(ulong venueId, ulong fixtureId);
    Task DeleteAsync(ulong venueId, ulong fixtureId);
}

public class FixtureListService(SportsVenuesContext db, HybridCache cache) : IFixtureListService
{
    private readonly SportsVenuesContext _db = db;

    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<VenueFixtureDto>> GetAllAsync(CancellationToken token = default)
    {
        return await _cache.GetOrCreateAsync(
            "venue-fixtures-all",
            async cancel => await _db.VenueFixtures
                .Select(vf => new VenueFixtureDto(vf.VenueId, vf.FixtureId, vf.CreatedAt))
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

    public async Task<IEnumerable<VenueFixtureDto>> GetByVenueAsync(ulong venueId)
    {
        return await _db.VenueFixtures
            .Where(vf => vf.VenueId == venueId)
            .Select(vf => new VenueFixtureDto(vf.VenueId, vf.FixtureId, vf.CreatedAt))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(ulong venueId, ulong fixtureId)
    {
        var venueExists = await _db.Venues.AnyAsync(v => v.Id == venueId);
        if (!venueExists)
            throw new KeyNotFoundException($"Venue {venueId} not found.");

        var fixtureExists = await _db.Fixtures.AnyAsync(f => f.Id == fixtureId);
        if (!fixtureExists)
            throw new KeyNotFoundException($"Fixture {fixtureId} not found.");

        var alreadyLinked = await _db.VenueFixtures
            .AnyAsync(vf => vf.VenueId == venueId && vf.FixtureId == fixtureId);
        if (alreadyLinked)
            throw new InvalidOperationException($"Fixture {fixtureId} is already linked to venue {venueId}.");

        var link = new VenueFixtureEntity
        {
            VenueId = venueId,
            FixtureId = fixtureId,
            CreatedAt = DateTime.UtcNow
        };

        await _db.VenueFixtures.AddAsync(link);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ulong venueId, ulong fixtureId)
    {
        var link = await _db.VenueFixtures
            .FirstOrDefaultAsync(vf => vf.VenueId == venueId && vf.FixtureId == fixtureId);

        if (link == null)
            throw new KeyNotFoundException($"No link found between venue {venueId} and fixture {fixtureId}.");

        _db.VenueFixtures.Remove(link);
        await _db.SaveChangesAsync();
    }
}
