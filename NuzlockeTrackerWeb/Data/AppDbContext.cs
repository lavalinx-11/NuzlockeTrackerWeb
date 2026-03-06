using Microsoft.EntityFrameworkCore;
using NuzlockeTrackerWeb.Components.GameData; // This points to where your MatchResult class is

namespace NuzlockeTrackerWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // This is the table that will store your history permanently
    public DbSet<MatchResult> Matches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Postgres needs to know how to handle your List<string> properties
        // We tell it to store them as 'jsonb' columns
        modelBuilder.Entity<MatchResult>(entity =>
        {
            entity.Property(e => e.Team1Names).HasColumnType("jsonb");
            entity.Property(e => e.Team2Names).HasColumnType("jsonb");
            entity.Property(e => e.Team1Roster).HasColumnType("jsonb");
            entity.Property(e => e.Team2Roster).HasColumnType("jsonb");
            entity.Property(e => e.BanList).HasColumnType("jsonb");
        });
    }
}