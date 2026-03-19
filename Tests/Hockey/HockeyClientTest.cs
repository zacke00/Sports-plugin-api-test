using Sport.App.Hockey;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sport.App.Tests.Hockey;

public class HockeyClientTest
{
    private readonly HockeyClient _client;

    public HockeyClientTest()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        configurationBuilder.AddUserSecrets<HockeyClientTest>();
        IConfiguration configuration = configurationBuilder.Build();

        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddHockey();
        var app = serviceCollection.BuildServiceProvider();

        _client = app.GetRequiredService<HockeyClient>();
    }

    [Fact]
    public async Task GetGamesAsync()
    {
        var result = await _client.GetGamesAsync(81, 2024);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Response);
    }
}
