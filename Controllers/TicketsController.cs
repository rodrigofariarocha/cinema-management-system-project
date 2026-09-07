using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RochaCinema.Data;
using RochaCinema.Models;
using System.Security.Claims;

namespace RochaCinema.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public TicketsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Tickets/MyTickets
    public async Task<IActionResult> MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var tickets = await _context.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Theater)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();

        return View(tickets);
    }

    // GET: Tickets/Purchase/5
    public async Task<IActionResult> Purchase(int id)
    {
        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screening == null)
        {
            return NotFound();
        }

        if (screening.AvailableSeats <= 0)
        {
            TempData["Error"] = "Não há lugares disponíveis para esta sessão.";
            return RedirectToAction("MovieDetails", "Home", new { id = screening.MovieId });
        }

        if (screening.DateTime < DateTime.Now)
        {
            TempData["Error"] = "Esta sessão já passou.";
            return RedirectToAction("MovieDetails", "Home", new { id = screening.MovieId });
        }

        return View(screening);
    }

    // POST: Tickets/ConfirmPurchase
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPurchase(int screeningId, string seatNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == screeningId);

        if (screening == null)
        {
            return NotFound();
        }

        if (screening.AvailableSeats <= 0)
        {
            TempData["Error"] = "Não há lugares disponíveis para esta sessão.";
            return RedirectToAction("Purchase", new { id = screeningId });
        }

        // Check if seat is already taken
        var seatTaken = await _context.Tickets
            .AnyAsync(t => t.ScreeningId == screeningId && 
                          t.SeatNumber == seatNumber && 
                          t.Status == "Ativo");

        if (seatTaken)
        {
            TempData["Error"] = "Este lugar já está ocupado. Por favor escolha outro.";
            return RedirectToAction("Purchase", new { id = screeningId });
        }

        var ticket = new Ticket
        {
            UserId = userId!,
            ScreeningId = screeningId,
            SeatNumber = seatNumber,
            PurchaseDate = DateTime.Now,
            Status = "Ativo",
            PricePaid = screening.Price
        };

        screening.AvailableSeats--;

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Bilhete comprado com sucesso! Lugar: {seatNumber}";
        return RedirectToAction("TicketConfirmation", new { id = ticket.Id });
    }

    // GET: Tickets/TicketConfirmation/5
    public async Task<IActionResult> TicketConfirmation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var ticket = await _context.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Theater)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // POST: Tickets/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var ticket = await _context.Tickets
            .Include(t => t.Screening)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (ticket == null)
        {
            return NotFound();
        }

        if (ticket.Status != "Cancelado")
        {
            ticket.Status = "Cancelado";
            ticket.Screening.AvailableSeats++;
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Bilhete cancelado com sucesso.";
        }

        return RedirectToAction(nameof(MyTickets));
    }
}
