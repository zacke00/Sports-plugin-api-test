using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Sport.App.FormulaOne;

public class FormulaOneClient
{
    private readonly HttpClient _http;
    private readonly IOptions<FormulaOneOptions> _options;

    private static readonly JsonSerializerOptions _jsonSerializer = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FormulaOneClient(HttpClient http, IOptions<FormulaOneOptions> options)
    {
        _http = http;
        _options = options;

        _http.BaseAddress = _options.Value.BaseUrl;
        _http.DefaultRequestHeaders.Add("x-apisports-key", _options.Value.ApiKey);
    }

    public async Task<FormulaOneRacesResponse?> GetRacesByRangeAsync(string grandPrixName, string season, string? date)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException("ApiSports API key is not configured. Set 'FormulaOne:ApiKey' in environment, user-secrets or .env.");
        }

        var url = $"races?season={season}";
        if (!string.IsNullOrWhiteSpace(grandPrixName)) url += $"&granPrixName={grandPrixName}";
        if (!string.IsNullOrWhiteSpace(date)) url += $"&date={date}";

        HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        requestMessage.Headers.Add("Accept", "application/json");

        var response = await _http.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var snippet = content?.Length > 500 ? content[..500] + "..." : content;
            throw new HttpRequestException($"ApiSports returned {(int)response.StatusCode} {response.ReasonPhrase}: {snippet}");
        }

        var data = await response.Content.ReadFromJsonAsync<FormulaOneRacesResponse>(_jsonSerializer);
        return data;
    }
}

public record FormulaOneRacesResponse(
    string? Get,
    Dictionary<string, string>? Parameters,
    int Results,
    List<FormulaOneRaceItem>? Response,
    JsonElement? Errors
);

public class FormulaOneRaceItem
{
    public long Id { get; init; }
    public FormulaOneCompetition? Competition { get; init; }
    public string? Date { get; init; }
    public string? Status { get; init; }
}

public class FormulaOneCompetition
{
    public string? Name { get; init; }
}

public record FormulaOneLocation(
    string? Country,
    string? City
);

public record FormulaOneCircuit(
    int Id,
    string? Name,
    string? Image
);
