using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

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

            // Configure relationships
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Fee>()
                .HasOne(f => f.Participant)
                .WithMany(p => p.Fees)
                .HasForeignKey(f => f.PartID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Participant)
                .WithMany(p => p.Teams)
                .HasForeignKey(t => t.PartID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Track)
                .WithMany(t => t.Events)
                .HasForeignKey(e => e.TrackID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TeamEvent>()
                .HasOne(te => te.Team)
                .WithMany(t => t.TeamEvents)
                .HasForeignKey(te => te.TeamID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamEvent>()
                .HasOne(te => te.Event)
                .WithMany(e => e.TeamEvents)
                .HasForeignKey(te => te.EventID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}