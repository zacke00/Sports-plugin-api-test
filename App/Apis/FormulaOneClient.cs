using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Sport.App.Apis
{
    public class FormulaOneClient{
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public FormulaOneClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiSports:ApiKey"] ?? string.Empty;
            if (!string.IsNullOrEmpty(_apiKey))
                _http.DefaultRequestHeaders.Add("x-apisports-key", _apiKey);
        }

        public async Task<FormulaOneRacesResponse?> GetRacesByRangeAsync(string granPrixName, string season, string? date)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("ApiSports API key is not configured. set ApiSports:ApiKey in environment, user-secrets or .env.");
            }

            var url = $"races?granPrixName={granPrixName}&season={season}";
            if (!string.IsNullOrWhiteSpace(date)) url += $"&date={date}";
            using var resp = await _http.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                var snippet = content?.Length > 500 ? content[..500] + "..." : content;
                throw new HttpRequestException($"ApiSports returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {snippet}");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try 
            {
                var result = JsonSerializer.Deserialize<FormulaOneRacesResponse>(content, options);
                return result;
            }
            catch (JsonException je)
            {
                throw new InvalidOperationException($"Failed to deserialize Formula One JSON response: {je.Message}. Body (truncated): {(content?.Length > 500 ? content[..500] + "..." : content)}");
            }
        }
    }

    public class FormulaOneRacesResponse
    {
        public string? get { get; set; }
        public Dictionary<string, string>? parameter { get; set; }
        public List<FormulaOneRaceItem>? response { get; set; }
        public int results { get; set; }
        public JsonElement? errors { get; set; }
    }

    public class FormulaOneRaceItem
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? Status { get; set; }
        public string? Circuit { get; set; }
        public string? Season { get; set; }
    }

    public class FormulaOneRaceDetail
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? Status { get; set; }
        public string? Circuit { get; set; }
        public string? Season { get; set; }
    }
    public class ExternalGranPrixName
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
    }
}