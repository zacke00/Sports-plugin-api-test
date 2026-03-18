using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sport.App.Apis;
using Sport.App.Data;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Services{
    public class VenuesService : IVenuesService
    {
    private readonly SportsVenuesScaffoldContext _db;
    private readonly VenuesClient _api;

    public VenuesService(SportsVenuesScaffoldContext db, VenuesClient api)
        {
            _db = db;
            _api = api;
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
                // Insert new venue
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
}