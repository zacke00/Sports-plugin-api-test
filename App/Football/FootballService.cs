using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Football;

public interface IFootballService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task<fixture?> GetByIdAsync(ulong id);
    Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to);
}

public class FootballService : IFootballService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly FootballClient _client;

    public FootballService(SportsVenuesScaffoldContext db, FootballClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<fixture>> GetAllAsync()
    {
        return await _db.fixtures.Where(f => f.sport_type == "football").AsNoTracking().ToListAsync();
    }

    public async Task<fixture?> GetByIdAsync(ulong id)
    {
        return await _db.fixtures.AsNoTracking().FirstOrDefaultAsync(f => f.id == id && f.sport_type == "football");
    }

    public async Task SyncFixturesRangeAsync(int league, int season, DateOnly? from, DateOnly? to)
    {
        // Delete existing fixtures in the date range only if a range is provided.
        if (from is not null && to is not null )
        {
            var parsedFrom = from.Value.ToDateTime(TimeOnly.MinValue);
            var parsedTo = to.Value.ToDateTime(TimeOnly.MinValue);

            var toDelete = await _db.fixtures.Where(f => f.starts_at >= parsedFrom && f.starts_at <= parsedTo && f.sport_type == "football").ToListAsync();
            if (toDelete.Any())
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
            var teams = item.Teams;
            var goals = item.Goals;

            var providerFixtureId = fx.Id.ToString();

            var model = new fixture
            {
                provider = "api-sports",
                provider_fixture_id = providerFixtureId,
                sport_type = "football",
                league_name = item.League?.Name,
                starts_at = DateTime.Parse(fx.Date!).ToUniversalTime(),
                home_team_name = teams?.Home?.Name,
                away_team_name = teams?.Away?.Name,
                home_score = goals?.Home,
                away_score = goals?.Away,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.provider == "api-sports" &&
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
