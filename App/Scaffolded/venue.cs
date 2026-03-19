namespace Sport.App.Models.Scaffolded;

public partial class Venue
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public DateTime Created_at { get; set; }

    public DateTime Updated_at { get; set; }

    public virtual ICollection<Venue_fixture> Venue_fixtures { get; set; } = new List<Venue_fixture>();
}
