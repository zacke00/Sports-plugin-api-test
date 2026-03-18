namespace Sport.App.Football;

public class FootballOptions
{
    public const string ConfigSection = "Football";
    
    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }
}