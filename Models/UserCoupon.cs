using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaRocha.Models;

public class UserCoupon
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int CouponId { get; set; }
    
    public DateTime IssuedAt { get; set; } = DateTime.Now;
    
    public bool IsUsed { get; set; } = false;
    
    public DateTime? UsedAt { get; set; }
    
    public int? ReservationId { get; set; }
    
    public string? QRCodeData { get; set; }
    
    // Navigation
    public IdentityUser User { get; set; } = null!;
    public Coupon Coupon { get; set; } = null!;
    public Reservation? Reservation { get; set; }
}
