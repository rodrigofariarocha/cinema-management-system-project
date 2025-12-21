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
    
    // Loyalty and Coupon system
    public DbSet<UserLoyalty> UserLoyalties { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<UserCoupon> UserCoupons { get; set; }

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
        
        // Configure Loyalty System
        modelBuilder.Entity<UserLoyalty>(entity =>
        {
            entity.HasOne(ul => ul.User)
                  .WithMany()
                  .HasForeignKey(ul => ul.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(ul => ul.UserId).IsUnique();
        });
        
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
        });
        
        modelBuilder.Entity<UserCoupon>(entity =>
        {
            entity.HasOne(uc => uc.User)
                  .WithMany()
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(uc => uc.Coupon)
                  .WithMany(c => c.UserCoupons)
                  .HasForeignKey(uc => uc.CouponId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(uc => uc.Reservation)
                  .WithMany()
                  .HasForeignKey(uc => uc.ReservationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
