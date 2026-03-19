using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Sport.App.Handball;

public class HandballClient
{
    private readonly HttpClient _http;
    private readonly IOptions<HandballOptions> _options;

    private static readonly JsonSerializerOptions _jsonSerializer = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HandballClient(HttpClient http, IOptions<HandballOptions> options)
    {
        _http = http;
        _options = options;

        _http.BaseAddress = _options.Value.BaseUrl;
        _http.DefaultRequestHeaders.Add("x-apisports-key", _options.Value.ApiKey);

    }

    public async Task<HandballGamesResponse?> GetGamesByDateAsync(DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException("ApiSports API key is not configured. Set 'Handball:ApiKey' in environment, user-secrets or .env.");
        }

        var url = $"games?date={date:yyyy-MM-dd}";

        HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        requestMessage.Headers.Add("Accept", "application/json");

        var response = await _http.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var snippet = content?.Length > 500 ? content[..500] + "..." : content;
            throw new HttpRequestException($"Hockey Api Returned {(int) response.StatusCode} {response.ReasonPhrase}: {snippet} ");
        }
        
        var result = await response.Content.ReadFromJsonAsync<HandballGamesResponse>(_jsonSerializer);
        return result;
    }
}

public record HandballGamesResponse(
    string? Get,
    Dictionary<string, string>? Parameters,
    int Results,
    List<HandballGameItem>? Response,
    JsonElement? Errors
);

public record HandballGameItem(
    long Id,
    string? Date,
    string? Time,
    long Timestamp,
    string? Timezone,
    HandballLeague? League,
    HandballTeams? Teams,
    HandballScores? Scores
);

public record HandballLeague(int Id, string? Name, string? Type, string? Logo, int Season);
public record HandballTeams(HandballTeam? Home, HandballTeam? Away);
public record HandballTeam(int Id, string? Name, string? Logo);
public record HandballScores(int? Home, int? Away);
