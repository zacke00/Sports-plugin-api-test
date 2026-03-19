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
            .FirstOrDefaultAsync(v => v.Name == normalizedName);

        if (existing != null)
        {
            existing.Name = normalizedName;
            existing.Location = location ?? existing.Location;
            existing.Address = address ?? existing.Address;
            existing.Phone = phone ?? existing.Phone;
            existing.Updated_at = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }
        else
        {
            var v = new Venue
            {
                Name = normalizedName,
                Location = location,
                Address = address,
                Phone = phone,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };

            await _db.venues.AddAsync(v);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = v.Id }, v);
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(ulong id, Venue input)
    {
        var v = await _db.venues.FindAsync(id);
        if (v == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input?.Name)) v.Name = input.Name;
        v.Location = input?.Location ?? v.Location;
        v.Address = input?.Address ?? v.Address;
        v.Phone = input?.Phone ?? v.Phone;
        v.Updated_at = DateTime.UtcNow;

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
