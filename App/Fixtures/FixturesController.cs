using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sport.App.Data;

namespace Sport.App.Fixtures;

[ApiController]
[Route("api/[controller]")]
public class FixturesController(SportsVenuesScaffoldContext db) : ControllerBase
{
    private readonly SportsVenuesScaffoldContext _db = db;

    /// <summary>
    /// Get all fixtures regardless of sport type.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var fixtures = await _db.venue_fixtures
            .AsNoTracking()
            .ToListAsync();

        return Ok(fixtures);
    }

    /// <summary>
    /// Get a single fixture by ID regardless of sport type.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var fixture = await _db.fixtures
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.id == id);

        if (fixture is null)
            return NotFound(new { error = $"Fixture {id} not found." });

        return Ok(fixture);
    }
}
