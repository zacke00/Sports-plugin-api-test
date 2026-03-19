using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.FormulaOne;

public interface IFormulaOneService
{
    Task<IEnumerable<fixture>> GetAllAsync();
    Task<fixture?> GetByIdAsync(ulong id);
    Task SyncFixturesRangeAsync(int season, DateOnly? from, DateOnly? to);
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

    public async Task<fixture?> GetByIdAsync(ulong id)
    {
        return await _db.fixtures.AsNoTracking().FirstOrDefaultAsync(f => f.id == id && f.sport_type == "formula-1");
    }

    public async Task SyncFixturesRangeAsync( int season, DateOnly? from, DateOnly? to)
    {
        // Delete existing fixtures in the date range only if a range is provided.
        if (from is not null && to is not null)
        {
            var parsedFrom = from.Value.ToDateTime(TimeOnly.MinValue);
            var parsedTo = to.Value.ToDateTime(TimeOnly.MinValue);

            var toDelete = await _db.fixtures.Where(f => f.starts_at >= parsedFrom && f.starts_at <= parsedTo && f.sport_type == "formula-1").ToListAsync();
            if (toDelete.Any())
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

            var model = new fixture
            {
                provider = "api-sports-formula1",
                provider_fixture_id = providerFixtureId,
                sport_type = "formula-1",
                race_name = r.Competition?.Name,
                starts_at = DateTimeOffset.Parse(r.Date!).UtcDateTime,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            // Upsert: if exists, update; else add
            var exists = await _db.fixtures.FirstOrDefaultAsync(f =>
                f.provider == "api-sports-formula1" &&
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
