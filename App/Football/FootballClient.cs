using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Sport.App.Football;

public class FootballClient
{
    private readonly HttpClient _http;
    private readonly IOptions<FootballOptions> _options;

    private static readonly JsonSerializerOptions _jsonSerializer = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FootballClient(HttpClient http, IOptions<FootballOptions> options)
    {
        _http = http;
        _options = options;
        
        _http.BaseAddress = _options.Value.BaseUrl;
        _http.DefaultRequestHeaders.Add("x-apisports-key", _options.Value.ApiKey);
    }

    public async Task<ExternalFixturesResponse?> GetFixturesByRangeAsync(int league, int season, DateOnly? from, DateOnly? to)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException("ApiSports API key is not configured. Set 'ApiSports:ApiKey' in environment, user-secrets or .env.");
        }

        var url = $"fixtures?league={league}&season={season}";

        if (from is not null)
            url += $"&from={from:yyyy-MM-dd}";

        if (to is not null)
            url += $"&to={to:yyyy-MM-dd}";

        HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        requestMessage.Headers.Add("Accept", "application/json");
        
        var response = await _http.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // include a truncated body to help debugging
            var snippet = content?.Length > 500 ? content[..500] + "..." : content;
            throw new HttpRequestException($"ApiSports returned {(int)response.StatusCode} {response.ReasonPhrase}: {snippet}");
        }

        var raw = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<ExternalFixturesResponse>(raw, _jsonSerializer);
        return data;
    }
}

public class ExternalFixturesResponse
{
    public string? Get { get; init; }
    public Dictionary<string, string>? Parameters { get; init; }
    public int Results { get; init; }
    public List<ExternalFixtureItem>? Response { get; init; }
    public JsonElement? Errors { get; init; }
}

public class ExternalFixtureItem
{
    public ExternalFixtureDetail? Fixture { get; init; }
    public ExternalLeague? League { get; init; }
    public ExternalTeams? Teams { get; init; }
    public ExternalGoals? Goals { get; init; }
}

public class ExternalFixtureDetail
{
    public long Id { get; init; }
    public string? Date { get; init; }
}

public class ExternalLeague
{
    public int Id { get; init; }
    public string? Name { get; init; }
}

public class ExternalTeams
{
    public ExternalTeam? Home { get; init; }
    public ExternalTeam? Away { get; init; }
}

public class ExternalTeam
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
}

public class ExternalGoals
{
    public int? Home { get; init; }
    public int? Away { get; init; }
}
