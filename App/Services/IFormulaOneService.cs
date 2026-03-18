using System;
using System.Threading.Tasks;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Services
{
    public interface IFormulaOneService
    {
        Task<IEnumerable<fixture>> GetAllAsync();
        Task SyncRacesByDateAsync(string season, string? date);
    }
}