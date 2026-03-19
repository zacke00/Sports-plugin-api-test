using Microsoft.Extensions.DependencyInjection;

namespace Sport.App.VenueFixture;

public static class FixtureListExtensions
{
    public static IServiceCollection AddVenueFixture(this IServiceCollection services)
    {
        services.AddScoped<IFixtureListService, FixtureListService>();
        return services;
    }
}
