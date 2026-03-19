namespace Sport.App.Hockey;

public class HockeyOptions
{
    public const string ConfigSection = "Hockey";
    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }
}