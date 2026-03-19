using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Hockey;

public interface IHockeyService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task SyncGamesAsync(int league, int season);
}

public class HockeyService : IHockeyService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly HockeyClient _Client;

    public HockeyService(SportsVenuesScaffoldContext db, HockeyClient client)
    {
        _db = db;
        _Client = client;
    }

    public async Task<IEnumerable<fixture>> GetAllAsync()
    {
        return await _db.fixtures.Where(f => f.sport_type == "Hockey").AsNoTracking().ToListAsync();
    }
    public async Task SyncGamesAsync(int league, int season)
    {
        var external = await _Client.GetGamesAsync(league, season);
        if (external?.Response == null) return;

        foreach (var g in external.Response)
        {
            var model = new fixture
            {
                id = (ulong)g.Id,
                provider = "api-sports-hockey",
                provider_fixture_id = g.Id.ToString(),
                sport_type = "hockey",
                league_name = g.League?.Name,
                starts_at = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                home_team_name = g.Teams?.Home?.Name,
                away_team_name = g.Teams?.Away?.Name,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
            };

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