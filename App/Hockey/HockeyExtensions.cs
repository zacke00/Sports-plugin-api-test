
using Sport.App.Hockey;

public static class HockeyExtensions
{
    public static IServiceCollection AddHockey(this IServiceCollection services)
    {
        services.AddOptions<HockeyOptions>().BindConfiguration(HockeyOptions.ConfigSection);
        services.AddHttpClient<HockeyClient>();
        services.AddScoped<IHockeyService, HockeyService>();
        
        return services;
    }
}