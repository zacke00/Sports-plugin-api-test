namespace Sport.App.FormulaOne;

public class FormulaOneOptions
{
    public const string ConfigSection = "FormulaOne";

    public required string ApiKey { get; set; }
    public required Uri BaseUrl { get; set; }
}
