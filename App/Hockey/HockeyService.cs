using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Hockey;

public interface IHockeyService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task<fixture?> GetByIdAsync(ulong id);
    Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to);
}

public class HockeyService : IHockeyService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly HockeyClient _client;

    public HockeyService(SportsVenuesScaffoldContext db, HockeyClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<fixture>> GetAllAsync()
    {
        return await _db.fixtures.Where(f => f.sport_type == "hockey").AsNoTracking().ToListAsync();
    }

    public async Task<fixture?> GetByIdAsync(ulong id)
    {
        return await _db.fixtures.AsNoTracking().FirstOrDefaultAsync(f => f.id == id && f.sport_type == "hockey");
    }

    public async Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to)
    {
        // Delete existing fixtures in the date range only if a range is provided.
        if (from is not null && to is not null)
        {
            var parsedFrom = from.Value.ToDateTime(TimeOnly.MinValue);
            var parsedTo = to.Value.ToDateTime(TimeOnly.MinValue);

            var toDelete = await _db.fixtures.Where(f => f.starts_at >= parsedFrom && f.starts_at <= parsedTo && f.sport_type == "hockey").ToListAsync();
            if (toDelete.Any())
            {
                _db.fixtures.RemoveRange(toDelete);
                await _db.SaveChangesAsync();
            }
        }

        // Fetch from external API
        var external = await _client.GetGamesAsync(league, season, from, to);
        if (external?.Response == null) return;

        foreach (var g in external.Response)
        {
            var providerFixtureId = g.Id.ToString();

            var model = new fixture
            {
                provider = "api-sports-hockey",
                provider_fixture_id = providerFixtureId,
                sport_type = "hockey",
                league_name = g.League?.Name,
                starts_at = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                home_team_name = g.Teams?.Home?.Name,
                away_team_name = g.Teams?.Away?.Name,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.provider == "api-sports-hockey" &&
                f.provider_fixture_id == providerFixtureId);
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
    }
}