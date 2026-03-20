namespace Sport.App.Data.Entities;

public class Fixture
{
    public ulong Id { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderFixtureId { get; set; } = null!;

    public string SportType { get; set; } = null!;

    public string? LeagueName { get; set; }

    public DateTime StartsAt { get; set; }

    public string? HomeTeamName { get; set; }

    public string? AwayTeamName { get; set; }

    public string? RaceName { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<VenueFixture> VenueFixtures { get; set; } = new List<VenueFixture>();
}
