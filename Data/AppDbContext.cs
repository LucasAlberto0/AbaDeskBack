using AbaDeskBack.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbaDeskBack.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketHistory> TicketHistories { get; set; } = null!;
    public DbSet<TicketComment> TicketComments { get; set; } = null!;
    public DbSet<TestCase> TestCases { get; set; } = null!;
    public DbSet<Homologation> Homologations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Email unique index
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Ticket Relationships
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.HomologationResponsibleUser)
            .WithMany()
            .HasForeignKey(t => t.HomologationResponsibleUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ticket Indexes
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Status);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Priority);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.CreatedAt);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.CreatedByUserId);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.AssignedToUserId);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.ProtocolNumber);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.SystemName);

        // TicketHistory Relationships
        modelBuilder.Entity<TicketHistory>()
            .HasOne(th => th.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(th => th.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketHistory>()
            .HasOne(th => th.User)
            .WithMany(u => u.History)
            .HasForeignKey(th => th.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TicketComment Relationships
        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TestCase Relationships
        modelBuilder.Entity<TestCase>()
            .HasOne(tc => tc.Ticket)
            .WithMany(t => t.TestCases)
            .HasForeignKey(tc => tc.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestCase>()
            .HasOne(tc => tc.CreatedByUser)
            .WithMany(u => u.TestCases)
            .HasForeignKey(tc => tc.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Homologation Relationships
        modelBuilder.Entity<Homologation>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.Homologations)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Homologation>()
            .HasOne(h => h.RequestedByUser)
            .WithMany()
            .HasForeignKey(h => h.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Homologation>()
            .HasOne(h => h.ResponsibleUser)
            .WithMany(u => u.Homologations)
            .HasForeignKey(h => h.ResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Homologation>()
            .HasOne(h => h.DecidedByUser)
            .WithMany()
            .HasForeignKey(h => h.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
