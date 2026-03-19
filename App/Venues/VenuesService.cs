using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Venues;

public interface IVenuesService
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone);
}

public class VenuesService : IVenuesService
{
    private readonly SportsVenuesScaffoldContext _db;
    private readonly VenuesClient _client;

    public VenuesService(SportsVenuesScaffoldContext db, VenuesClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _db.venues.AsNoTracking().ToListAsync();
    }

    public async Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone)
    {
        var existingVenue = await _db.venues.FirstOrDefaultAsync(v => v.Name == name);

        if (existingVenue != null)
        {
            existingVenue.Name = name;
            existingVenue.Location = location;
            existingVenue.Address = address;
            existingVenue.Phone = phone;
            existingVenue.Updated_at = DateTime.UtcNow;
            _db.venues.Update(existingVenue);
        }
        else
        {
            var newVenue = new Venue
            {
                Name = name,
                Location = location,
                Address = address,
                Phone = phone,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };

            await _db.venues.AddAsync(newVenue);
        }

        await _db.SaveChangesAsync();
    }
}
