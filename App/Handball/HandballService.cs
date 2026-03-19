using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Handball;

public interface IHandballService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task SyncGamesByDateAsync(string date);
}

public class HandballService : IHandballService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly HandballClient _client;

    public HandballService(SportsVenuesScaffoldContext db, HandballClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<fixture>> GetAllAsync()
    {
        return await _db.fixtures.Where(f => f.sport_type == "handball").AsNoTracking().ToListAsync();
    }

    public async Task SyncGamesByDateAsync(string date)
    {
        // validate simple format yyyy-MM-dd
        if (!System.Text.RegularExpressions.Regex.IsMatch(date, "^\\d{4}-\\d{2}-\\d{2}$"))
            throw new InvalidOperationException("Date must be in YYYY-MM-DD format.");

        var external = await _client.GetGamesByDateAsync(date);
        if (external?.Response == null) return;

        foreach (var g in external.Response)
        {
            var model = new fixture
            {
                id = (ulong)g.Id,
                provider = "api-sports-handball",
                provider_fixture_id = g.Id.ToString(),
                sport_type = "handball",
                league_name = g.League?.Name,
                starts_at = DateTimeOffset.Parse(g.Date!).UtcDateTime,
                home_team_name = g.Teams?.Home?.Name,
                away_team_name = g.Teams?.Away?.Name,
                home_score = g.Scores?.Home,
                away_score = g.Scores?.Away,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
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
