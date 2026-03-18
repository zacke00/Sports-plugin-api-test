using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sport.App.Apis
{
    public class HandballClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public HandballClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiSports:ApiKey"] ?? string.Empty;
            if (!string.IsNullOrEmpty(_apiKey))
                _http.DefaultRequestHeaders.Add("x-apisports-key", _apiKey);
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HandballGamesResponse?> GetGamesByDateAsync(string date)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("ApiSports API key is not configured. Set 'ApiSports:ApiKey' in environment, user-secrets or .env.");

            var url = $"games?date={date}";
            using var resp = await _http.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                var snippet = content?.Length > 500 ? content[..500] + "..." : content;
                throw new HttpRequestException($"Handball API returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {snippet}");
            }

            var trimmed = (content ?? string.Empty).TrimStart();
            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[")))
            {
                var snippet = trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed;
                throw new InvalidOperationException($"Handball API did not return JSON. Body starts with: {snippet}");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            HandballGamesResponse? result;
            try
            {
                result = JsonSerializer.Deserialize<HandballGamesResponse>(content, options);
            }
            catch (JsonException je)
            {
                throw new InvalidOperationException($"Failed to deserialize Handball JSON response: {je.Message}. Body (truncated): {(content?.Length > 500 ? content[..500] + "..." : content)}");
            }

            if (result != null && result.errors.HasValue)
            {
                var e = result.errors.Value;
                string msg = string.Empty;
                if (e.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in e.EnumerateObject())
                    {
                        if (!string.IsNullOrEmpty(msg)) msg += "; ";
                        msg += prop.Value.GetString() ?? prop.Value.ToString();
                    }
                }
                else if (e.ValueKind == JsonValueKind.Array)
                {
                    var parts = new List<string>();
                    foreach (var el in e.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.String)
                            parts.Add(el.GetString()!);
                        else
                            parts.Add(el.ToString() ?? string.Empty);
                    }
                    msg = string.Join("; ", parts.Where(s => !string.IsNullOrEmpty(s)));
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    throw new InvalidOperationException($"Handball API returned error(s): {msg}");
                }
            }

            return result;
        }
    }

    public class HandballGamesResponse
    {
        public string? get { get; set; }
        public Dictionary<string,string>? parameters { get; set; }
        public List<HandballGameItem>? response { get; set; }
        public int results { get; set; }
        public JsonElement? errors { get; set; }
    }

    public class HandballGameItem
    {
        public long id { get; set; }
        public string? date { get; set; }
        public string? time { get; set; }
        public long timestamp { get; set; }
        public string? timezone { get; set; }
        public HandballLeague? league { get; set; }
        public HandballTeams? teams { get; set; }
        public HandballScores? scores { get; set; }
    }

    public class HandballLeague { public int id { get; set; } public string? name { get; set; } public string? type { get; set; } public string? logo { get; set; } public int season { get; set; } }
    public class HandballTeams { public HandballTeam? home { get; set; } public HandballTeam? away { get; set; } }
    public class HandballTeam { public int id { get; set; } public string? name { get; set; } public string? logo { get; set; } }
    public class HandballScores { public int? home { get; set; } public int? away { get; set; } }
}
