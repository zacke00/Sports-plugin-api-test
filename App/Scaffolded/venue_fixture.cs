namespace Sport.App.Models.Scaffolded;

public partial class Venue_fixture
{
    public ulong Venue_id { get; set; }

    public ulong Fixture_id { get; set; }

    public DateTime Created_at { get; set; }

    public virtual Fixture Fixture { get; set; } = null!;

    public virtual Venue Venue { get; set; } = null!;
}
