using Microsoft.AspNetCore.Mvc;
using Sport.App.Services;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Controllers
{
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
                Console.WriteLine($"Synced fixtures for {season} on {date}");
                Console.WriteLine($"Successfully synced fixtures for {season} on {date}");
                return Accepted();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error syncing fixtures for {season} on {date}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}