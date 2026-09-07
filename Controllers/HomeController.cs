using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RochaCinema.Data;
using RochaCinema.Models;

namespace RochaCinema.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .Include(m => m.Screenings.Where(s => s.DateTime > DateTime.Now))
                .ThenInclude(s => s.Theater)
            .Where(m => m.Screenings.Any(s => s.DateTime > DateTime.Now))
            .OrderByDescending(m => m.ReleaseDate)
            .Take(12)
            .ToListAsync();
        
        return View(movies);
    }

    public async Task<IActionResult> MovieDetails(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Screenings.Where(s => s.DateTime > DateTime.Now))
                .ThenInclude(s => s.Theater)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
