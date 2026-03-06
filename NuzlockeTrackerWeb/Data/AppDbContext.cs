using Microsoft.EntityFrameworkCore;
using NuzlockeTrackerWeb.Components.GameData;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuzlockeTrackerWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MatchResult> Matches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Force the table name to lowercase so Postgres finds it easily
        modelBuilder.Entity<MatchResult>().ToTable("matches");

        modelBuilder.Entity<MatchResult>(entity =>
        {
            // Manually map every column to a lowercase/snake_case name
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MatchDate).HasColumnName("match_date");
            entity.Property(e => e.Team1Key).HasColumnName("team1_key");
            entity.Property(e => e.Team2Key).HasColumnName("team2_key");
            entity.Property(e => e.Team1Rounds).HasColumnName("team1_rounds");
            entity.Property(e => e.Team2Rounds).HasColumnName("team2_rounds");
            entity.Property(e => e.WinningTeamSide).HasColumnName("winning_team_side");

            // Store Lists as jsonb columns (lowercase names)
            entity.Property(e => e.Team1Names).HasColumnType("jsonb").HasColumnName("team1_names");
            entity.Property(e => e.Team2Names).HasColumnType("jsonb").HasColumnName("team2_names");
            entity.Property(e => e.Team1Roster).HasColumnType("jsonb").HasColumnName("team1_roster");
            entity.Property(e => e.Team2Roster).HasColumnType("jsonb").HasColumnName("team2_roster");
            entity.Property(e => e.BanList).HasColumnType("jsonb").HasColumnName("ban_list");
        });
    }
}