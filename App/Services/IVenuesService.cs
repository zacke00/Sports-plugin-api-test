using System.Collections.Generic;
using System.Threading.Tasks;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Services
{
    public interface IVenuesService
    {
        Task<IEnumerable<venue>> GetAllAsync();
        Task CreateOrUpdateVenueAsync(string name, string location, string address, string phone);
    }
}
