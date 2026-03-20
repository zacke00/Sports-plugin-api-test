using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Data.Entities;

namespace Sport.App.Venues;

public interface IVenuesService
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone);
}

public class VenuesService : IVenuesService
{
    private readonly SportsVenuesContext _db;
    private readonly VenuesClient _client;

    public VenuesService(SportsVenuesContext db, VenuesClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _db.Venues.AsNoTracking().ToListAsync();
    }

    public async Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone)
    {
        var existingVenue = await _db.Venues.FirstOrDefaultAsync(v => v.Name == name);

        if (existingVenue != null)
        {
            existingVenue.Name = name;
            existingVenue.Location = location;
            existingVenue.Address = address;
            existingVenue.Phone = phone;
            existingVenue.UpdatedAt = DateTime.UtcNow;
            _db.Venues.Update(existingVenue);
        }
        else
        {
            var newVenue = new Venue
            {
                Name = name,
                Location = location,
                Address = address,
                Phone = phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.Venues.AddAsync(newVenue);
        }

        await _db.SaveChangesAsync();
    }
}
