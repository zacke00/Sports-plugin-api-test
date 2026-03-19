namespace Sport.App.Handball;

public static class HandballExtensions
{
    public static IServiceCollection AddHandball(this IServiceCollection services)
    {
        services.AddOptions<HandballOptions>().BindConfiguration(HandballOptions.ConfigSection);
        services.AddHttpClient<HandballClient>();
        services.AddScoped<IHandballService, HandballService>();

        return services;
    }
}
