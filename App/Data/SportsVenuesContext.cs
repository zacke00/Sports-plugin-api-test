using Microsoft.EntityFrameworkCore;
using Sport.App.Data.Entities;
using VenueFixtureEntity = Sport.App.Data.Entities.VenueFixture;

namespace Sport.App.Data;

public class SportsVenuesContext : DbContext
{
    public SportsVenuesContext(DbContextOptions<SportsVenuesContext> options)
        : base(options)
    {
    }

    public DbSet<Fixture> Fixtures => Set<Fixture>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<VenueFixtureEntity> VenueFixtures => Set<VenueFixtureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Fixture>(entity =>
        {
            entity.ToTable("fixtures");

            entity.HasKey(e => e.Id);

            // Global query filter: exclude soft-deleted fixtures by default
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => new { e.SportType, e.LeagueName })
                  .HasDatabaseName("idx_fixtures_sport_league");

            entity.HasIndex(e => e.StartsAt)
                  .HasDatabaseName("idx_fixtures_starts_at");

            entity.HasIndex(e => new { e.Provider, e.ProviderFixtureId })
                  .IsUnique()
                  .HasDatabaseName("ux_fixture_external");

            // Composite index for the common sync query: WHERE sport_type = X AND starts_at BETWEEN ...
            entity.HasIndex(e => new { e.SportType, e.StartsAt })
                  .HasDatabaseName("idx_fixtures_sport_starts");

            // Index on soft-delete column for filtered queries
            entity.HasIndex(e => e.DeletedAt)
                  .HasDatabaseName("idx_fixtures_deleted_at");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Provider)
                  .HasColumnName("provider")
                  .HasMaxLength(64)
                  .IsRequired();

            entity.Property(e => e.ProviderFixtureId)
                  .HasColumnName("provider_fixture_id")
                  .HasMaxLength(128)
                  .IsRequired();

            entity.Property(e => e.SportType)
                  .HasColumnName("sport_type")
                  .HasMaxLength(64)
                  .IsRequired();

            entity.Property(e => e.LeagueName)
                  .HasColumnName("league_name")
                  .HasMaxLength(128);

            entity.Property(e => e.StartsAt)
                  .HasColumnName("starts_at")
                  .HasColumnType("datetime");

            entity.Property(e => e.HomeTeamName)
                  .HasColumnName("home_team_name")
                  .HasMaxLength(255);

            entity.Property(e => e.HomeTeamLogo)
                  .HasColumnName("home_team_logo")
                  .HasMaxLength(512);

            entity.Property(e => e.AwayTeamName)
                  .HasColumnName("away_team_name")
                  .HasMaxLength(255);

            entity.Property(e => e.AwayTeamLogo)
                  .HasColumnName("away_team_logo")
                  .HasMaxLength(512);

            entity.Property(e => e.RaceName)
                  .HasColumnName("race_name")
                  .HasMaxLength(255);

            entity.Property(e => e.HomeScore)
                  .HasColumnName("home_score");

            entity.Property(e => e.AwayScore)
                  .HasColumnName("away_score");

            entity.Property(e => e.DeletedAt)
                  .HasColumnName("deleted_at")
                  .HasColumnType("datetime");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnType("timestamp");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.ToTable("venues");

            entity.HasKey(e => e.Id);

            // Index for lookup-by-name used in upsert operations
            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("idx_venues_name");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.Location)
                  .HasColumnName("location")
                  .HasMaxLength(255);

            entity.Property(e => e.Address)
                  .HasColumnName("address")
                  .HasMaxLength(255);

            entity.Property(e => e.Phone)
                  .HasColumnName("phone")
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnType("timestamp");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnType("timestamp");
        });

        modelBuilder.Entity<VenueFixtureEntity>(entity =>
        {
            entity.ToTable("venue_fixtures");

            entity.HasKey(e => new { e.VenueId, e.FixtureId });

            entity.HasIndex(e => e.FixtureId)
                  .HasDatabaseName("idx_vf_fixture_id");

            entity.Property(e => e.VenueId)
                  .HasColumnName("venue_id");

            entity.Property(e => e.FixtureId)
                  .HasColumnName("fixture_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnType("timestamp");

            entity.HasOne(e => e.Fixture)
                  .WithMany(f => f.VenueFixtures)
                  .HasForeignKey(e => e.FixtureId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_vf_fixture");

            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.VenueFixtures)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("fk_vf_venue");
        });
    }
}
