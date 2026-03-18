using Microsoft.AspNetCore.Mvc;
using Sport.App.Services;

namespace Sport.App.Football;

[ApiController]
[Route("api/[controller]")]
public class FootballController : ControllerBase
{
    private readonly IFootballService _svc;
    public FootballController(IFootballService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _svc.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var item = await _svc.GetByIdAsync((ulong)id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(int league, string season, string? from, string? to)
    {
        try
        {
            await _svc.SyncFixturesRangeAsync(league, season, from ?? null, to ?? null);
            return Accepted();
        }
        catch (InvalidOperationException ioe)
        {
            return BadRequest(new { error = ioe.Message });
        }
        catch (HttpRequestException hre)
        {
            return StatusCode(502, new { error = hre.Message });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
