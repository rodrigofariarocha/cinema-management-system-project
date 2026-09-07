using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RochaCinema.Data;
using RochaCinema.Models;

namespace RochaCinema.Controllers;

[Authorize(Roles = "Admin")]
public class TheatersController : Controller
{
    private readonly ApplicationDbContext _context;

    public TheatersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Theaters
    public async Task<IActionResult> Index()
    {
        var theaters = await _context.Theaters
            .OrderBy(t => t.Name)
            .ToListAsync();
        return View(theaters);
    }

    // GET: Theaters/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var theater = await _context.Theaters
            .Include(t => t.Screenings)
                .ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (theater == null)
        {
            return NotFound();
        }

        return View(theater);
    }

    // GET: Theaters/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Theaters/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Capacity")] Theater theater)
    {
        if (ModelState.IsValid)
        {
            _context.Add(theater);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(theater);
    }

    // GET: Theaters/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var theater = await _context.Theaters.FindAsync(id);
        if (theater == null)
        {
            return NotFound();
        }
        return View(theater);
    }

    // POST: Theaters/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Capacity")] Theater theater)
    {
        if (id != theater.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(theater);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TheaterExists(theater.Id))
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
        return View(theater);
    }

    // GET: Theaters/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var theater = await _context.Theaters
            .FirstOrDefaultAsync(m => m.Id == id);
        if (theater == null)
        {
            return NotFound();
        }

        return View(theater);
    }

    // POST: Theaters/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var theater = await _context.Theaters.FindAsync(id);
        if (theater != null)
        {
            _context.Theaters.Remove(theater);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool TheaterExists(int id)
    {
        return _context.Theaters.Any(e => e.Id == id);
    }
}
