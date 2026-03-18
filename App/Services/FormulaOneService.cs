using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;
using Sport.App.Apis;

namespace Sport.App.Services
{
    public class FormulaOneService : IFormulaOneService
    {
    private readonly SportsVenuesScaffoldContext _db;
    private readonly FormulaOneClient _api;

    public FormulaOneService(SportsVenuesScaffoldContext db, FormulaOneClient api)
        {
            _db = db;
            _api = api;
        }

        public async Task<IEnumerable<fixture>> GetAllAsync()
            {
                var fixtures = await _db.fixtures
                    .Where(f => f.sport_type == "formula-1")
                    .AsNoTracking()
                    .ToListAsync();

                return fixtures;
            }
        public async Task SyncRacesByDateAsync(string season, string? date)
        {
            if(string.IsNullOrWhiteSpace(season))
                throw new InvalidOperationException("season is required");
            
            var external = await _api.GetRacesByRangeAsync(string.Empty, season, date);
            if(external?.response == null || external.response.Count == 0)
            {
                Console.WriteLine($"No fixtures found for {season} on {date}");
                return;
            }

            foreach (var r in external.response)
            {
                var model = new fixture
                {
                    id = (ulong)r.Id,
                    provider = "api-sports-formula1",
                    provider_fixture_id = r.Id.ToString(),
                    sport_type = "formula-1",
                    league_name = r.Season,
                    starts_at = DateTimeOffset.Parse(r.Date).UtcDateTime,
                };

            }

        }

    }

}