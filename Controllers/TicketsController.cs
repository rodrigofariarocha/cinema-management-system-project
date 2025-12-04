using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Data;
using CinemaRocha.Models;
using System.Security.Claims;

namespace CinemaRocha.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Sessions)
                .ThenInclude(s => s.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // GET: Tickets/SelectSeats/5
        [Authorize]
        public async Task<IActionResult> SelectSeats(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .ThenInclude(r => r.Seats)
                .Include(s => s.Reservations)
                .ThenInclude(r => r.Seats)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: Tickets/Book
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int sessionId, string selectedSeats)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                return RedirectToAction(nameof(SelectSeats), new { sessionId });
            }

            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var seatIds = selectedSeats.Split(',').Select(int.Parse).ToList();
            
            var seats = await _context.Seats.Where(s => seatIds.Contains(s.Id)).ToListAsync();

            var reservation = new Reservation
            {
                UserId = userId,
                SessionId = sessionId,
                ReservationDate = DateTime.Now,
                Seats = seats,
                TotalPrice = seats.Count * session.Price
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
        }

        // GET: Tickets/Confirmation/5
        [Authorize]
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Session)
                .ThenInclude(s => s.Movie)
                .Include(r => r.Session)
                .ThenInclude(s => s.Room)
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // GET: Tickets/MyTickets
        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _context.Reservations
                .Include(r => r.Session)
                .ThenInclude(s => s.Movie)
                .Include(r => r.Session)
                .ThenInclude(s => s.Room)
                .Include(r => r.Seats)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }
    }
}
