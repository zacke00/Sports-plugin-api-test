using Microsoft.AspNetCore.Mvc;
using Sport.App.Services;

namespace Sport.App.Controllers
{
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

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(string? date)
    {
        try
        {
            await _svc.SyncGamesByDateAsync(date ?? string.Empty);
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
}
