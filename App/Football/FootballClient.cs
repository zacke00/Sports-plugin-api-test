using System.Net.Mime;
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

    public async Task<ExternalFixturesResponse?> GetFixturesByRangeAsync(int league, string season, string? from, string? to)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException("ApiSports API key is not configured. Set 'ApiSports:ApiKey' in environment, user-secrets or .env.");
        }

        var url = $"fixtures?league={league}&season={season}";

        if (!string.IsNullOrWhiteSpace(from)) url += $"&from={from}";
        if (!string.IsNullOrWhiteSpace(to)) url += $"&to={to}";

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

        var data = await response.Content.ReadFromJsonAsync<ExternalFixturesResponse>(_jsonSerializer);
        return data;
    }
}

public record ExternalFixturesResponse(
    string? Get,
    Dictionary<string, string>? Parameters,
    int Results,
    List<ExternalFixtureItem>? Response,
    JsonElement? Errors
);

public record ExternalFixtureItem(
    ExternalFixtureDetail? Fixture,
    ExternalLeague? League,
    ExternalTeams? Teams,
    ExternalGoals? Goals
);

public record ExternalFixtureDetail(long Id, string? Date);
public record ExternalLeague(int Id, string? Name);
public record ExternalTeams(ExternalTeam? Home, ExternalTeam? Away);
public record ExternalTeam(int Id, string? Name, string? Logo);
public record ExternalGoals(int? Home, int? Away);
