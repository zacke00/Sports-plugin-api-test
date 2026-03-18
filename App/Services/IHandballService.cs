using System.Collections.Generic;
using System.Threading.Tasks;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Services
{
    public interface IHandballService
    {
        Task<IEnumerable<fixture>> GetAllAsync();
        Task SyncGamesByDateAsync(string date);
    }
}
