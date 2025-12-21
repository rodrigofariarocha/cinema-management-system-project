using System.ComponentModel.DataAnnotations;

namespace CinemaRocha.Models;

public enum CouponType
{
    FreeTicket,
    PercentDiscount,
    FixedDiscount
}

public class Coupon
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public CouponType Type { get; set; }
    
    public decimal DiscountValue { get; set; }
    
    [Required]
    public DateTime ValidFrom { get; set; }
    
    [Required]
    public DateTime ValidTo { get; set; }
    
    public int? MaxUses { get; set; } // null = unlimited
    
    public int CurrentUses { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Navigation
    public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
