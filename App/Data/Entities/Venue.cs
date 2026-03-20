namespace Sport.App.Data.Entities;

public class Venue
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<VenueFixture> VenueFixtures { get; set; } = new List<VenueFixture>();
}
