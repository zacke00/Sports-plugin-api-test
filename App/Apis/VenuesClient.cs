using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Apis
{
    public class VenuesClient
    {
        private readonly HttpClient _httpClient;

        public VenuesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<venue> GetVenueByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/venues/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<venue>(json);
            }
            return null;
        }
    }
}