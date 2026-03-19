using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.VenueFixture;

public record VenueFixtureDto(
    ulong VenueId,
    ulong FixtureId,
    DateTime CreatedAt
);

public interface IFixtureListService
{
    Task<IEnumerable<VenueFixtureDto>> GetByVenueAsync(ulong venueId);
    Task AddAsync(ulong venueId, ulong fixtureId);
    Task DeleteAsync(ulong venueId, ulong fixtureId);
}

public class FixtureListService(SportsVenuesScaffoldContext db) : IFixtureListService
{
    private readonly SportsVenuesScaffoldContext _db = db;

    public async Task<IEnumerable<VenueFixtureDto>> GetByVenueAsync(ulong venueId)
    {
        return await _db.venue_fixtures
            .Where(vf => vf.Venue_id == venueId)
            .Select(vf => new VenueFixtureDto(vf.Venue_id, vf.Fixture_id, vf.Created_at))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(ulong venueId, ulong fixtureId)
    {
        var venueExists = await _db.venues.AnyAsync(v => v.Id == venueId);
        if (!venueExists)
            throw new KeyNotFoundException($"Venue {venueId} not found.");

        var fixtureExists = await _db.fixtures.AnyAsync(f => f.id == fixtureId);
        if (!fixtureExists)
            throw new KeyNotFoundException($"Fixture {fixtureId} not found.");

        var alreadyLinked = await _db.venue_fixtures
            .AnyAsync(vf => vf.Venue_id == venueId && vf.Fixture_id == fixtureId);
        if (alreadyLinked)
            throw new InvalidOperationException($"Fixture {fixtureId} is already linked to venue {venueId}.");

        var link = new Venue_fixture
        {
            Venue_id = venueId,
            Fixture_id = fixtureId,
            Created_at = DateTime.UtcNow
        };

        await _db.venue_fixtures.AddAsync(link);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ulong venueId, ulong fixtureId)
    {
        var link = await _db.venue_fixtures
            .FirstOrDefaultAsync(vf => vf.Venue_id == venueId && vf.Fixture_id == fixtureId);

        if (link == null)
            throw new KeyNotFoundException($"No link found between venue {venueId} and fixture {fixtureId}.");

        _db.venue_fixtures.Remove(link);
        await _db.SaveChangesAsync();
    }
}
