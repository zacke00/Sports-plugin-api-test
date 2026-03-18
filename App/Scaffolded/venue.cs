using System;
using System.Collections.Generic;

namespace Sport.App.Models.Scaffolded;

public partial class venue
{
    public ulong id { get; set; }

    public string name { get; set; } = null!;

    public string? location { get; set; }

    public string? address { get; set; }

    public string? phone { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<venue_fixture> venue_fixtures { get; set; } = new List<venue_fixture>();
}
