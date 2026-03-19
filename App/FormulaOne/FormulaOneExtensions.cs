namespace Sport.App.FormulaOne;

public static class FormulaOneExtensions
{
    public static IServiceCollection AddFormulaOne(this IServiceCollection services)
    {
        services.AddOptions<FormulaOneOptions>().BindConfiguration(FormulaOneOptions.ConfigSection);
        services.AddHttpClient<FormulaOneClient>();
        services.AddScoped<IFormulaOneService, FormulaOneService>();

        return services;
    }
}
