using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Apis;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Football;

public interface IFootballService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task<fixture?> GetByIdAsync(ulong id);
    Task SyncFixturesRangeAsync(int league, string season, string from, string to);
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

    public async Task SyncFixturesRangeAsync(int league, string season, string? from, string? to)
    {
        // Default to today's date if missing
        if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
        {
            var today = DateTime.UtcNow.Date;
            from = today.ToString("yyyy-MM-dd");
            to = today.ToString("yyyy-MM-dd");
        }
        else if (string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
        {
            // if only 'to' provided, set from = to
            from = to;
        }
        else if (!string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
        {
            // if only from provided, set to = from
            to = from;
        }

        // Delete existing fixtures in the date range (hard-delete for now).
        var parsedFrom = DateTime.Parse(from!);
        var parsedTo = DateTime.Parse(to!).AddDays(1).AddTicks(-1);

        var toDelete = await _db.fixtures.Where(f => f.starts_at >= parsedFrom && f.starts_at <= parsedTo && f.sport_type == "football").ToListAsync();
        if (toDelete.Any())
        {
            _db.fixtures.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
        }

        // Fetch from external API
        var external = await _client.GetFixturesByRangeAsync(league, season, from, to);
        if (external?.response == null) return;

        foreach (var item in external.response)
        {
            var fx = item.fixture;
            var teams = item.teams;
            var goals = item.goals;

            var model = new fixture
            {
                id = (ulong)fx.id,
                provider = "api-sports",
                provider_fixture_id = fx.id.ToString(),
                sport_type = "football",
                league_name = item.league?.name,
                starts_at = DateTime.Parse(fx.date).ToUniversalTime(),
                home_team_name = teams?.home?.name,
                away_team_name = teams?.away?.name,
                home_score = goals?.home,
                away_score = goals?.away,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FindAsync(model.id);
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
