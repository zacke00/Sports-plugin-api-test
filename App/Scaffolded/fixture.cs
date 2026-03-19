namespace Sport.App.Models.Scaffolded;

public partial class Fixture
{
    public ulong id { get; set; }

    public string Provider { get; set; } = null!;

    public string Provider_fixture_id { get; set; } = null!;

    public string Sport_type { get; set; } = null!;

    public string? League_name { get; set; }

    public DateTime Starts_at { get; set; }

    public string? Home_team_name { get; set; }

    public string? Away_team_name { get; set; }

    public string? Race_name { get; set; }

    public int? Home_score { get; set; }

    public int? Away_score { get; set; }

    public DateTime? Deleted_at { get; set; }

    public DateTime Created_at { get; set; }

    public DateTime Updated_at { get; set; }

    public virtual ICollection<Venue_fixture> Venue_fixtures { get; set; } = new List<Venue_fixture>();
}
