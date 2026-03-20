namespace Sport.App.FormulaOne;

public class FormulaOneOptions
{
    public const string ConfigSection = "FormulaOne";

    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }

    /// <summary>
    /// Seasons (YYYY) to sync. When empty the sync service automatically
    /// uses the current calendar year and the previous one.
    /// </summary>
    public int[] Seasons { get; set; } = [];
}
