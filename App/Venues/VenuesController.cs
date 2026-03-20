using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Data.Entities;

namespace Sport.App.Venues;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly SportsVenuesContext _db;

    public VenuesController(SportsVenuesContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.Venues.AsNoTracking().ToListAsync());

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(ulong id)
    {
        var v = await _db.Venues.FindAsync(id);
        return v == null ? NotFound() : Ok(v);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateVenueAsync(string name, string? location, string? address, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("name is required");

        var normalizedName = name.Trim();

        var existing = await _db.Venues
            .FirstOrDefaultAsync(v => v.Name == normalizedName);

        if (existing != null)
        {
            existing.Name = normalizedName;
            existing.Location = location ?? existing.Location;
            existing.Address = address ?? existing.Address;
            existing.Phone = phone ?? existing.Phone;
            existing.UpdatedAt = DateTime.UtcNow;

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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.Venues.AddAsync(v);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = v.Id }, v);
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(ulong id, Venue input)
    {
        var v = await _db.Venues.FindAsync(id);
        if (v == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input?.Name)) v.Name = input.Name;
        v.Location = input?.Location ?? v.Location;
        v.Address = input?.Address ?? v.Address;
        v.Phone = input?.Phone ?? v.Phone;
        v.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(v);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var v = await _db.Venues.FindAsync(id);
        if (v == null) return NotFound();
        _db.Venues.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
