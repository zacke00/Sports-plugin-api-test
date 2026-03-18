

namespace Sport.App.Football;

public static class FootballExtensions
{
    public static IServiceCollection AddFootball(this IServiceCollection services)
    {
        services.AddOptions<FootballOptions>().BindConfiguration(FootballOptions.ConfigSection);
        services.AddHttpClient<FootballClient>();
        services.AddScoped<IFootballService, FootballService>();

        return services;
    }
}