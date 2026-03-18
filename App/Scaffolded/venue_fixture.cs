using System;
using System.Collections.Generic;

namespace Sport.App.Models.Scaffolded;

public partial class venue_fixture
{
    public ulong venue_id { get; set; }

    public ulong fixture_id { get; set; }

    public DateTime created_at { get; set; }

    public virtual fixture fixture { get; set; } = null!;

    public virtual venue venue { get; set; } = null!;
}
