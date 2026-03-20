namespace Sport.App.Hockey;

public class HockeyOptions
{
    public const string ConfigSection = "Hockey";
    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }

    /// <summary>
    /// League IDs to sync fixtures for (e.g. 1 = NHL).
    /// </summary>
    public int[] Leagues { get; set; } = [];

    /// <summary>
    /// Seasons (YYYY) to sync. When empty the sync service automatically
    /// uses the current calendar year and the previous one.
    /// </summary>
    public int[] Seasons { get; set; } = [];
}