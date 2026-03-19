using Microsoft.AspNetCore.Mvc;

namespace Sport.App.VenueFixture;

[ApiController]
[Route("api/venue-fixtures")]
public class FixtureListController(IFixtureListService svc) : ControllerBase
{
    private readonly IFixtureListService _svc = svc;
    
    [HttpGet("{venueId:long}")]
    public async Task<IActionResult> GetByVenue(ulong venueId)
    {
        var results = await _svc.GetByVenueAsync(venueId);
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Add(ulong venueId, ulong fixtureId)
    {
        try
        {
            await _svc.AddAsync(venueId, fixtureId);
            return Created(string.Empty, new { venueId,fixtureId });
        }
        catch (KeyNotFoundException knfe)
        {
            return NotFound(new { error = knfe.Message });
        }
        catch (InvalidOperationException ioe)
        {
            return Conflict(new { error = ioe.Message });
        }
    }

    // DELETE api/venue-fixtures/{venueId}/{fixtureId}
    // Removes a fixture link from a venue.
    [HttpDelete("{venueId:long}/{fixtureId:long}")]
    public async Task<IActionResult> Delete(ulong venueId, ulong fixtureId)
    {
        try
        {
            await _svc.DeleteAsync(venueId, fixtureId);
            return NoContent();
        }
        catch (KeyNotFoundException knfe)
        {
            return NotFound(new { error = knfe.Message });
        }
    }
}


