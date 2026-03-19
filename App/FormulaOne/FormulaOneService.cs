using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.FormulaOne;

public interface IFormulaOneService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task SyncRacesByDateAsync(string season, string? date);
}

public class FormulaOneService : IFormulaOneService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly FormulaOneClient _client;

    public FormulaOneService(SportsVenuesScaffoldContext db, FormulaOneClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<fixture>> GetAllAsync()
    {
        return await _db.fixtures
            .Where(f => f.sport_type == "formula-1")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SyncRacesByDateAsync(string season, string? date)
    {
        if (string.IsNullOrWhiteSpace(season))
            throw new InvalidOperationException("season is required");

        var external = await _client.GetRacesByRangeAsync(string.Empty, season, date);
        if (external?.Response == null || external.Response.Count == 0)
        {
            Console.WriteLine($"No fixtures found for {season} on {date}");
            return;
        }

        foreach (var r in external.Response)
        {
            var model = new fixture
            {
                provider = "api-sports-formula1",
                provider_fixture_id = r.Id.ToString(),
                sport_type = "formula-1",
                race_name = r.Competition?.Name,
                starts_at = DateTimeOffset.Parse(r.Date!).UtcDateTime,
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
