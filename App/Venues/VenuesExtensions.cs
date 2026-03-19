namespace Sport.App.Venues;

public static class VenuesExtensions
{
    public static IServiceCollection AddVenues(this IServiceCollection services)
    {
        services.AddHttpClient<VenuesClient>();
        services.AddScoped<IVenuesService, VenuesService>();

        return services;
    }
}
