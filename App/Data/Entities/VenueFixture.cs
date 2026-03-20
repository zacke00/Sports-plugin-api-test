namespace Sport.App.Data.Entities;

public class VenueFixture
{
    public ulong VenueId { get; set; }

    public ulong FixtureId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Fixture Fixture { get; set; } = null!;

    public Venue Venue { get; set; } = null!;
}
