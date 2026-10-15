using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data;

/// <summary>
/// Database context for the RaceDay application, managing all entity relationships and configurations.
/// </summary>
public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Fee> Fees { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<TeamEvent> TeamEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.UserID);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Participants)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant Configuration
        modelBuilder.Entity<Participant>()
            .HasKey(p => p.PartID);
        
        modelBuilder.Entity<Participant>()
            .HasMany(p => p.Fees)
            .WithOne(f => f.Participant)
            .HasForeignKey(f => f.PartID)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Participant>()
            .HasMany(p => p.Teams)
            .WithOne(t => t.Participant)
            .HasForeignKey(t => t.PartID)
            .OnDelete(DeleteBehavior.Cascade);

        // Fee Configuration
        modelBuilder.Entity<Fee>()
            .HasKey(f => f.FeeID);

        // Team Configuration
        modelBuilder.Entity<Team>()
            .HasKey(t => t.TeamID);
        
        modelBuilder.Entity<Team>()
            .HasMany(t => t.TeamEvents)
            .WithOne(te => te.Team)
            .HasForeignKey(te => te.TeamID)
            .OnDelete(DeleteBehavior.Cascade);

        // Track Configuration
        modelBuilder.Entity<Track>()
            .HasKey(t => t.TrackID);
        
        modelBuilder.Entity<Track>()
            .HasMany(t => t.Events)
            .WithOne(e => e.Track)
            .HasForeignKey(e => e.TrackID)
            .OnDelete(DeleteBehavior.Restrict);

        // Event Configuration
        modelBuilder.Entity<Event>()
            .HasKey(e => e.EventID);
        
        modelBuilder.Entity<Event>()
            .HasMany(e => e.TeamEvents)
            .WithOne(te => te.Event)
            .HasForeignKey(te => te.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // TeamEvent Configuration
        modelBuilder.Entity<TeamEvent>()
            .HasKey(te => te.EventTeamID);
    }
}