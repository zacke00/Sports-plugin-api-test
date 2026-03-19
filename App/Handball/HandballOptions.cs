namespace Sport.App.Handball;

public class HandballOptions
{
    public const string ConfigSection = "Handball";

    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }
}
