using System.Text.Json;
using Sport.App.Hockey;

namespace Sport.App.Tests.Hockey;

public class HockeyGamesResponseTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize()
    {
        var json = """
            {
                "get": "games",
                "parameters": {
                    "league": "81",
                    "season": "2024"
                },
                "errors": [],
                "results": 297,
                "response": [
                    {
                        "id": 376018,
                        "date": "2024-10-05T15:10:00+00:00",
                        "time": "15:10",
                        "timestamp": 1728141000,
                        "timezone": "UTC",
                        "week": "1",
                        "timer": null,
                        "status": {
                            "long": "Finished",
                            "short": "FT"
                        },
                        "country": {
                            "id": 9,
                            "name": "France",
                            "code": "FR",
                            "flag": "https:\/\/media.api-sports.io\/flags\/fr.svg"
                        },
                        "league": {
                            "id": 81,
                            "name": "D1",
                            "type": "League",
                            "logo": "https:\/\/media.api-sports.io\/hockey\/leagues\/81.png",
                            "season": 2024
                        },
                        "teams": {
                            "home": {
                                "id": 972,
                                "name": "Neuilly Sur Marne",
                                "logo": "https:\/\/media.api-sports.io\/hockey\/teams\/972.png"
                            },
                            "away": {
                                "id": 970,
                                "name": "Mont Blanc",
                                "logo": "https:\/\/media.api-sports.io\/hockey\/teams\/970.png"
                            }
                        },
                        "scores": {
                            "home": 10,
                            "away": 2
                        },
                        "periods": {
                            "first": "2-1",
                            "second": "3-1",
                            "third": "5-0",
                            "overtime": null,
                            "penalties": null
                        },
                        "events": false
                    }
                ]
            }
        """;

        var result = JsonSerializer.Deserialize<HockeyGamesResponse>(json, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal("games", result.Get);
        Assert.Equal(297, result.Results);
        Assert.NotNull(result.Parameters);
        Assert.Equal("81", result.Parameters["league"]);
        Assert.Equal("2024", result.Parameters["season"]);

        Assert.NotNull(result.Response);
        Assert.Single(result.Response);

        var game = result.Response[0];
        Assert.Equal(376018, game.Id);
        Assert.Equal("2024-10-05T15:10:00+00:00", game.Date);
        Assert.Equal("15:10", game.Time);
        Assert.Equal(1728141000, game.Timestamp);
        Assert.Equal("UTC", game.Timezone);

        Assert.NotNull(game.League);
        Assert.Equal(81, game.League.Id);
        Assert.Equal("D1", game.League.Name);
        Assert.Equal("League", game.League.Type);
        Assert.Equal("https://media.api-sports.io/hockey/leagues/81.png", game.League.Logo);
        Assert.Equal(2024, game.League.Season);

        Assert.NotNull(game.Teams);
        Assert.NotNull(game.Teams.Home);
        Assert.Equal(972, game.Teams.Home.Id);
        Assert.Equal("Neuilly Sur Marne", game.Teams.Home.Name);
        Assert.Equal("https://media.api-sports.io/hockey/teams/972.png", game.Teams.Home.Logo);
        Assert.Equal(970, game.Teams.Away.Id);
        Assert.Equal("Mont Blanc", game.Teams.Away.Name);
        Assert.Equal("https://media.api-sports.io/hockey/teams/970.png", game.Teams.Away.Logo);

        Assert.NotNull(game.Scores);
        Assert.Equal(10, game.Scores.Home);
        Assert.Equal(2, game.Scores.Away);
    }
}
