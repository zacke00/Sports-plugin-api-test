using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sport.App.Models.Scaffolded;

namespace Sport.App.Data;

public partial class SportsVenuesScaffoldContext : DbContext
{
    public SportsVenuesScaffoldContext(DbContextOptions<SportsVenuesScaffoldContext> options)
        : base(options)
    {
    }

    public virtual DbSet<fixture> fixtures { get; set; }

    public virtual DbSet<venue> venues { get; set; }

    public virtual DbSet<venue_fixture> venue_fixtures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<fixture>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.sport_type, e.league_name }, "idx_fixtures_sport_league");

            entity.HasIndex(e => e.starts_at, "idx_fixtures_starts_at");

            entity.HasIndex(e => new { e.provider, e.provider_fixture_id }, "ux_fixture_external").IsUnique();

            entity.Property(e => e.away_team_name).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.deleted_at).HasColumnType("datetime");
            entity.Property(e => e.home_team_name).HasMaxLength(255);
            entity.Property(e => e.race_name).HasMaxLength(255);
            entity.Property(e => e.league_name).HasMaxLength(128);
            entity.Property(e => e.provider).HasMaxLength(64);
            entity.Property(e => e.provider_fixture_id).HasMaxLength(128);
            entity.Property(e => e.sport_type).HasMaxLength(64);
            entity.Property(e => e.starts_at).HasColumnType("datetime");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<venue>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.address).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.location).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(50);
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<venue_fixture>(entity =>
        {
            entity.HasKey(e => new { e.venue_id, e.fixture_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.HasIndex(e => e.fixture_id, "idx_vf_fixture_id");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.fixture).WithMany(p => p.venue_fixtures)
                .HasForeignKey(d => d.fixture_id)
                .HasConstraintName("fk_vf_fixture");

            entity.HasOne(d => d.venue).WithMany(p => p.venue_fixtures)
                .HasForeignKey(d => d.venue_id)
                .HasConstraintName("fk_vf_venue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
