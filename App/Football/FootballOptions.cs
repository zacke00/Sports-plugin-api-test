namespace Sport.App.Football;

public class FootballOptions
{
    public const string ConfigSection = "Football";
    
    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }

    /// <summary>
    /// League IDs to sync fixtures for (e.g. 39 = Premier League, 140 = La Liga).
    /// </summary>
    public int[] Leagues { get; set; } = [];
}