using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sport.App.Data;

namespace Sport.App.Fixtures;

[ApiController]
[Route("api/[controller]")]
public class FixturesController(SportsVenuesContext db) : ControllerBase
{
    private readonly SportsVenuesContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var fixtures = await _db.Fixtures
            .AsNoTracking()
            .ToListAsync();

        return Ok(fixtures);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var fixture = await _db.Fixtures
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fixture is null)
            return NotFound(new { error = $"Fixture {id} not found." });

        return Ok(fixture);
    }
}
