using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaRocha.Models;

public class UserLoyalty
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public int TotalPoints { get; set; } = 0;
    
    public int AvailablePoints { get; set; } = 0;
    
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    
    // Navigation
    public IdentityUser User { get; set; } = null!;
    public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
