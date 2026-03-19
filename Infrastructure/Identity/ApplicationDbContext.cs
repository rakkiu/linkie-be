using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<WishwallMessage> WishwallMessages => Set<WishwallMessage>();
        public DbSet<WishwallAiLog> WishwallAiLogs => Set<WishwallAiLog>();
        public DbSet<ArFrame> ArFrames => Set<ArFrame>();
        public DbSet<FrameUsage> FrameUsages => Set<FrameUsage>();
        public DbSet<WishwallKeyword> WishwallKeywords => Set<WishwallKeyword>();
        public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
        public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
        public DbSet<UserEventStat> UserEventStats => Set<UserEventStat>();
        public DbSet<JwtToken> JwtTokens => Set<JwtToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enums stored as strings
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Event>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<WishwallMessage>()
                .Property(w => w.Sentiment)
                .HasConversion<string>();

            // WishwallMessage relationships
            modelBuilder.Entity<WishwallMessage>()
                .HasOne(w => w.Event)
                .WithMany(e => e.WishwallMessages)
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishwallMessage>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishwallMessages)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ArFrame relationships
            modelBuilder.Entity<ArFrame>()
                .HasOne(a => a.Event)
                .WithMany(e => e.ArFrames)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // FrameUsage relationships
            modelBuilder.Entity<FrameUsage>()
                .HasOne(f => f.Event)
                .WithMany(e => e.FrameUsages)
                .HasForeignKey(f => f.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FrameUsage>()
                .HasOne(f => f.User)
                .WithMany(u => u.FrameUsages)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FrameUsage>()
                .HasOne(f => f.Frame)
                .WithMany(a => a.FrameUsages)
                .HasForeignKey(f => f.FrameId)
                .OnDelete(DeleteBehavior.Cascade);

            // WishwallKeyword relationships
            modelBuilder.Entity<WishwallKeyword>()
                .HasOne(w => w.Event)
                .WithMany(e => e.WishwallKeywords)
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventParticipant relationships
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventParticipants)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.EventParticipants)
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SystemLog relationships
            modelBuilder.Entity<SystemLog>()
                .HasOne(s => s.Admin)
                .WithMany(u => u.SystemLogs)
                .HasForeignKey(s => s.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserEventStat relationships
            modelBuilder.Entity<UserEventStat>()
                .HasOne(us => us.Event)
                .WithMany(e => e.UserEventStats)
                .HasForeignKey(us => us.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserEventStat>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserEventStats)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // JwtToken relationships
            modelBuilder.Entity<JwtToken>()
                .HasOne(j => j.User)
                .WithMany(u => u.JwtTokens)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
