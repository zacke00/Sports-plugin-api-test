using System.Text.Json;
using Sport.App.Data.Entities;

namespace Sport.App.Venues;

public class VenuesClient
{
    private readonly HttpClient _httpClient;

    public VenuesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Venue?> GetVenueByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/venues/{id}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Venue>(json);
        }
        return null;
    }
}
