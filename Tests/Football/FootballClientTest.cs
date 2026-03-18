using Sport.App.Football;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sport.App.Tests.Football;

public class FootballClientTest
{
    private readonly FootballClient _client;

    public FootballClientTest()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        configurationBuilder.AddUserSecrets<FootballClientTest>();
        IConfiguration configuration = configurationBuilder.Build();

        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddFootball();
        var app = serviceCollection.BuildServiceProvider();

        _client = app.GetRequiredService<FootballClient>();
    }

    [Fact]
    public async Task GetFixturesByRangeAsync()
    {
        var result = await _client.GetFixturesByRangeAsync(39, "2024", null, null);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Response);
    }
}
