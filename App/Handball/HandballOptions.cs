namespace Sport.App.Handball;

public class HandballOptions
{
    public const string ConfigSection = "Handball";

    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }

    /// <summary>
    /// League IDs to sync fixtures for (e.g. 1 = Champions League).
    /// </summary>
    public int[] Leagues { get; set; } = [];

    /// <summary>
    /// How many days into the future to sync, starting from today.
    /// Today (day 0) is always included. Default is 8 days (today + 8 ahead).
    /// </summary>
    public int DaysAhead { get; set; } = 8;
}
