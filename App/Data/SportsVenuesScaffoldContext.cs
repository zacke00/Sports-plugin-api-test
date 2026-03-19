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

    public virtual DbSet<Fixture> fixtures { get; set; }

    public virtual DbSet<Venue> venues { get; set; }

    public virtual DbSet<Venue_fixture> venue_fixtures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Fixture>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.Sport_type, e.League_name }, "idx_fixtures_sport_league");

            entity.HasIndex(e => e.Starts_at, "idx_fixtures_starts_at");

            entity.HasIndex(e => new { e.Provider, e.Provider_fixture_id }, "ux_fixture_external").IsUnique();

            entity.Property(e => e.Away_team_name).HasMaxLength(255);
            entity.Property(e => e.Created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Deleted_at).HasColumnType("datetime");
            entity.Property(e => e.Home_team_name).HasMaxLength(255);
            entity.Property(e => e.Race_name).HasMaxLength(255);
            entity.Property(e => e.League_name).HasMaxLength(128);
            entity.Property(e => e.Provider).HasMaxLength(64);
            entity.Property(e => e.Provider_fixture_id).HasMaxLength(128);
            entity.Property(e => e.Sport_type).HasMaxLength(64);
            entity.Property(e => e.Starts_at).HasColumnType("datetime");
            entity.Property(e => e.Updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Venue_fixture>(entity =>
        {
            entity.HasKey(e => new { e.Venue_id, e.Fixture_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.HasIndex(e => e.Fixture_id, "idx_vf_fixture_id");

            entity.Property(e => e.Created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.Fixture).WithMany(p => p.Venue_fixtures)
                .HasForeignKey(d => d.Fixture_id)
                .HasConstraintName("fk_vf_fixture");

            entity.HasOne(d => d.Venue).WithMany(p => p.Venue_fixtures)
                .HasForeignKey(d => d.Venue_id)
                .HasConstraintName("fk_vf_venue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
