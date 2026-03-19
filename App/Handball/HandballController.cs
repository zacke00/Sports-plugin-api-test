using Microsoft.AspNetCore.Mvc;

namespace Sport.App.Handball;

[ApiController]
[Route("api/[controller]")]
public class HandballController : ControllerBase
{
    private readonly IHandballService _svc;
    public HandballController(IHandballService svc) => _svc = svc;

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
    public async Task<IActionResult> Sync(DateOnly date)
    {
        try
        {
            await _svc.SyncGamesByDateAsync(date);
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
