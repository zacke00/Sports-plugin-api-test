using System;
using System.Collections.Generic;

namespace Sport.App.Models.Scaffolded;

public partial class fixture
{
    public ulong id { get; set; }

    public string provider { get; set; } = null!;

    public string provider_fixture_id { get; set; } = null!;

    public string sport_type { get; set; } = null!;

    public string? league_name { get; set; }

    public DateTime starts_at { get; set; }

    public string? home_team_name { get; set; }

    public string? away_team_name { get; set; }

    public string? race_name { get; set; }

    public int? home_score { get; set; }

    public int? away_score { get; set; }

    public DateTime? deleted_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<venue_fixture> venue_fixtures { get; set; } = new List<venue_fixture>();
}
