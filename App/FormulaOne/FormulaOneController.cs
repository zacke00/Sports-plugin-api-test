using Microsoft.AspNetCore.Mvc;

namespace Sport.App.FormulaOne;

[ApiController]
[Route("api/[controller]")]
public class FormulaOneController : ControllerBase
{
    private readonly IFormulaOneService _svc;
    public FormulaOneController(IFormulaOneService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var item = await _svc.GetAllAsync();
        return Ok(item);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromQuery] string season, [FromQuery] string? date)
    {
        try
        {
            await _svc.SyncRacesByDateAsync(season, date);
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
