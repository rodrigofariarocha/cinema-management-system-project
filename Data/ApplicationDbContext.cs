using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Models;

namespace CinemaRocha.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for the core entities
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Seat> Seats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Movie entity
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasMany(m => m.Sessions)
                  .WithOne(s => s.Movie)
                  .HasForeignKey(s => s.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasMany(s => s.Reservations)
                  .WithOne(r => r.Session)
                  .HasForeignKey(r => r.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Reservation entity
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
