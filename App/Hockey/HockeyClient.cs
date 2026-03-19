
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Sport.App.Hockey;

public class HockeyClient
{
    private readonly HttpClient _http;
    private readonly IOptions<HockeyOptions> _options;

    private static readonly JsonSerializerOptions _jsonSerializer = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HockeyClient(HttpClient http, IOptions<HockeyOptions> options)
    {
        _http = http;
        _options = options;

        _http.BaseAddress = _options.Value.BaseUrl;
        _http.DefaultRequestHeaders.Add("x-apisports-key", _options.Value.ApiKey);

    }

    public async Task<HockeyGamesResponse?> GetGamesAsync(int league, int season)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException("ApiSports API key is not configured. Set 'Hockey:ApiKey' in environmentm user-secrets or .env ");
        }

        var url = $"games?league={league}&season={season}";

        HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        requestMessage.Headers.Add("accept", "application/json");

        var response = await _http.SendAsync(requestMessage);

        if(!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var snippet = content?.Length > 500 ? content[..500] + "..." : content;
            throw new HttpRequestException($"Hockey Api Returned {(int) response.StatusCode} {response.ReasonPhrase}: {snippet} ");
        }
        
        var result = await response.Content.ReadFromJsonAsync<HockeyGamesResponse>(_jsonSerializer);
        return result;
    }

}

public record HockeyGamesResponse(
    string? Get,
    Dictionary<string, string>? Parameters,
    int Results,
    List<HockeyGameItem>? Response,
    JsonElement? Errors
);

public record HockeyGameItem(
    long Id,
    string? Date,
    string? Time,
    long Timestamp,
    string? Timezone,
    HockeyLeague? League,
    HockeyTeams? Teams,
    HockeyScores? Scores
);

public record HockeyLeague(int Id, string? Name, string? Type, string? Logo, int Season);
public record HockeyTeams(HockeyTeam? Home, HockeyTeam Away);
public record HockeyTeam(int Id, string? Name, string? Logo);
public record HockeyScores(int? Home, int? Away);