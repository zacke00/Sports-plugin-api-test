
using Microsoft.AspNetCore.Mvc;

namespace Sport.App.Hockey;

[ApiController]
[Route("api/[controller]")]
public class HockeyController : ControllerBase
{
    private readonly IHockeyService _svc;
    public HockeyController(IHockeyService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _svc.GetAllAsync();
        return Ok(items);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(int league, int season)
    {
        try
        {
            await _svc.SyncGamesAsync(league, season);
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