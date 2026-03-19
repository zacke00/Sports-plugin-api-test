using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Venues;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly SportsVenuesScaffoldContext _db;

    public VenuesController(SportsVenuesScaffoldContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.venues.AsNoTracking().ToListAsync());

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(ulong id)
    {
        var v = await _db.venues.FindAsync(id);
        return v == null ? NotFound() : Ok(v);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateVenueAsync(string name, string? location, string? address, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("name is required");

        var normalizedName = name.Trim();

        var existing = await _db.venues
            .FirstOrDefaultAsync(v => v.name == normalizedName);

        if (existing != null)
        {
            if (!string.IsNullOrWhiteSpace(name)) existing.name = name.Trim();
            existing.location = location ?? existing.location;
            existing.address = address ?? existing.address;
            existing.phone = phone ?? existing.phone;
            existing.updated_at = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }
        else
        {
            var v = new venue
            {
                name = normalizedName,
                location = location,
                address = address,
                phone = phone,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await _db.venues.AddAsync(v);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = v.id }, v);
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(ulong id, venue input)
    {
        var v = await _db.venues.FindAsync(id);
        if (v == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input?.name)) v.name = input.name;
        v.location = input?.location ?? v.location;
        v.address = input?.address ?? v.address;
        v.phone = input?.phone ?? v.phone;
        v.updated_at = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(v);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var v = await _db.venues.FindAsync(id);
        if (v == null) return NotFound();
        _db.venues.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
