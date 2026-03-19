using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Venues;

public interface IVenuesService
{
    Task<IEnumerable<venue>> GetAllAsync();
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

    public async Task<IEnumerable<venue>> GetAllAsync()
    {
        return await _db.venues.ToListAsync();
    }

    public async Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone)
    {
        var existingVenue = await _db.venues.FirstOrDefaultAsync(v => v.name == name);

        if (existingVenue != null)
        {
            existingVenue.name = name;
            existingVenue.location = location;
            existingVenue.address = address;
            existingVenue.phone = phone;
            existingVenue.updated_at = DateTime.UtcNow;
            _db.venues.Update(existingVenue);
        }
        else
        {
            var newVenue = new venue
            {
                name = name,
                location = location,
                address = address,
                phone = phone,
                created_at = DateTime.UtcNow
            };

            await _db.venues.AddAsync(newVenue);
        }

        await _db.SaveChangesAsync();
    }
}
