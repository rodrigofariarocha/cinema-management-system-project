using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RochaCinema.Data;
using RochaCinema.Models;

namespace RochaCinema.Controllers;

[Authorize(Roles = "Admin")]
public class ScreeningsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ScreeningsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Screenings
    public async Task<IActionResult> Index()
    {
        var screenings = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .OrderByDescending(s => s.DateTime)
            .ToListAsync();
        return View(screenings);
    }

    // GET: Screenings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Tickets)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (screening == null)
        {
            return NotFound();
        }

        return View(screening);
    }

    // GET: Screenings/Create
    public IActionResult Create()
    {
        ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title");
        ViewData["TheaterId"] = new SelectList(_context.Theaters.OrderBy(t => t.Name), "Id", "Name");
        return View();
    }

    // POST: Screenings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MovieId,TheaterId,DateTime,Price")] Screening screening)
    {
        if (ModelState.IsValid)
        {
            // Set available seats to theater capacity
            var theater = await _context.Theaters.FindAsync(screening.TheaterId);
            if (theater != null)
            {
                screening.AvailableSeats = theater.Capacity;
            }

            _context.Add(screening);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", screening.MovieId);
        ViewData["TheaterId"] = new SelectList(_context.Theaters.OrderBy(t => t.Name), "Id", "Name", screening.TheaterId);
        return View(screening);
    }

    // GET: Screenings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var screening = await _context.Screenings.FindAsync(id);
        if (screening == null)
        {
            return NotFound();
        }

        ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", screening.MovieId);
        ViewData["TheaterId"] = new SelectList(_context.Theaters.OrderBy(t => t.Name), "Id", "Name", screening.TheaterId);
        return View(screening);
    }

    // POST: Screenings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,TheaterId,DateTime,Price,AvailableSeats")] Screening screening)
    {
        if (id != screening.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(screening);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScreeningExists(screening.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", screening.MovieId);
        ViewData["TheaterId"] = new SelectList(_context.Theaters.OrderBy(t => t.Name), "Id", "Name", screening.TheaterId);
        return View(screening);
    }

    // GET: Screenings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (screening == null)
        {
            return NotFound();
        }

        return View(screening);
    }

    // POST: Screenings/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var screening = await _context.Screenings.FindAsync(id);
        if (screening != null)
        {
            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ScreeningExists(int id)
    {
        return _context.Screenings.Any(e => e.Id == id);
    }
}
