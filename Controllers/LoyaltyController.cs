using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Data;
using CinemaRocha.Models;

namespace CinemaRocha.Controllers;

[Authorize]
public class LoyaltyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LoyaltyController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = user?.Email?.ToLower() == "admin@cinemarocha.com";
        
        var loyalty = await _context.UserLoyalties
            .FirstOrDefaultAsync(ul => ul.UserId == userId);
            
        if (loyalty == null)
        {
            loyalty = new UserLoyalty { UserId = userId! };
            _context.UserLoyalties.Add(loyalty);
            await _context.SaveChangesAsync();
        }
        
        List<UserCoupon> userCoupons;
        if (isAdmin)
        {
            userCoupons = new List<UserCoupon>();
        }
        else
        {
            userCoupons = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .Where(uc => uc.UserId == userId && !uc.IsUsed)
                .Where(uc => uc.Coupon.IsActive && uc.Coupon.ValidTo >= DateTime.Now)
                .OrderByDescending(uc => uc.IssuedAt)
                .ToListAsync();
        }
        
        ViewBag.UserCoupons = userCoupons;
        
        return View(loyalty);
    }
}
