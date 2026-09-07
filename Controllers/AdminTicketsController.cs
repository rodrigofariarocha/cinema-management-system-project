using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RochaCinema.Data;

namespace RochaCinema.Controllers;

[Authorize(Roles = "Admin")]
public class AdminTicketsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminTicketsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AdminTickets
    public async Task<IActionResult> Index()
    {
        var tickets = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Theater)
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();
        
        return View(tickets);
    }

    // GET: AdminTickets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Theater)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // POST: AdminTickets/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Screening)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound();
        }

        if (ticket.Status != "Cancelado")
        {
            ticket.Status = "Cancelado";
            ticket.Screening.AvailableSeats++;
            
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
