using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Data;
using CinemaRocha.Models;
using CinemaRocha.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CinemaRocha.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TmdbService _tmdbService;
        private readonly GeminiService _geminiService;

        public AdminController(ApplicationDbContext context, TmdbService tmdbService, GeminiService geminiService)
        {
            _context = context;
            _tmdbService = tmdbService;
            _geminiService = geminiService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new DashboardViewModel
            {
                TotalMovies = await _context.Movies.CountAsync(),
                TotalSessions = await _context.Sessions.CountAsync(),
                TotalReservations = await _context.Reservations.CountAsync(),
                TotalRevenue = await _context.Reservations.SumAsync(r => r.TotalPrice),
                
                RecentReservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Session)
                        .ThenInclude(s => s!.Movie)
                    .Include(r => r.Seats)
                    .OrderByDescending(r => r.ReservationDate)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentMovies = await _context.Movies
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentSessions = await _context.Sessions
                    .Include(s => s.Movie)
                    .Include(s => s.Room)
                    .OrderByDescending(s => s.StartTime)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentRooms = await _context.Rooms
                    .OrderByDescending(r => r.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        public async Task<IActionResult> ExportReport()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s!.Movie)
                .Include(r => r.Session)
                    .ThenInclude(s => s!.Room)
                .Include(r => r.Seats)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor("#0a0a0a");

                    page.Header().Column(col =>
                    {
                        col.Item().Text("CINEMA ROCHA").FontSize(32).Bold().FontColor("#ff3b30");
                        col.Item().Text("Relatório de Reservas").FontSize(18).FontColor("#ffffff");
                        col.Item().PaddingTop(5).Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#666666");
                        col.Item().PaddingTop(15).LineHorizontal(2).LineColor("#ff3b30");
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Container()
                                .Border(1).BorderColor("#333333")
                                .Background("#1a1a1a")
                                .Padding(15).CornerRadius(8)
                                .Column(c =>
                                {
                                    c.Item().Text("Total de Reservas").FontSize(10).FontColor("#666666");
                                    c.Item().Text(reservations.Count.ToString()).FontSize(24).Bold().FontColor("#ff3b30");
                                });

                            row.ConstantItem(20);

                            row.RelativeItem().Container()
                                .Border(1).BorderColor("#333333")
                                .Background("#1a1a1a")
                                .Padding(15).CornerRadius(8)
                                .Column(c =>
                                {
                                    c.Item().Text("Receita Total").FontSize(10).FontColor("#666666");
                                    c.Item().Text($"{reservations.Sum(r => r.TotalPrice):N2}€").FontSize(24).Bold().FontColor("#10b981");
                                });

                            row.ConstantItem(20);

                            row.RelativeItem().Container()
                                .Border(1).BorderColor("#333333")
                                .Background("#1a1a1a")
                                .Padding(15).CornerRadius(8)
                                .Column(c =>
                                {
                                    c.Item().Text("Bilhetes Vendidos").FontSize(10).FontColor("#666666");
                                    c.Item().Text(reservations.Sum(r => r.Seats.Count).ToString()).FontSize(24).Bold().FontColor("#60a5fa");
                                });
                        });

                        col.Item().Container()
                            .Background("#1a1a1a")
                            .Padding(10)
                            .Row(row =>
                            {
                                row.RelativeItem(2).Text("Data").FontSize(10).Bold().FontColor("#ffffff");
                                row.RelativeItem(3).Text("Cliente").FontSize(10).Bold().FontColor("#ffffff");
                                row.RelativeItem(3).Text("Filme").FontSize(10).Bold().FontColor("#ffffff");
                                row.RelativeItem(2).Text("Sala").FontSize(10).Bold().FontColor("#ffffff");
                                row.RelativeItem(1).AlignCenter().Text("Lugares").FontSize(10).Bold().FontColor("#ffffff");
                                row.RelativeItem(1).AlignRight().Text("Preço").FontSize(10).Bold().FontColor("#ffffff");
                            });

                        foreach (var reservation in reservations)
                        {
                            col.Item().Container()
                                .Border(1).BorderColor("#222222")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem(2).Text(reservation.ReservationDate.ToString("dd/MM/yy HH:mm")).FontSize(9).FontColor("#cccccc");
                                    row.RelativeItem(3).Text(reservation.User?.Email?.Split('@')[0] ?? "N/A").FontSize(9).FontColor("#cccccc");
                                    row.RelativeItem(3).Text(reservation.Session?.Movie?.Title ?? "N/A").FontSize(9).FontColor("#cccccc");
                                    row.RelativeItem(2).Text(reservation.Session?.Room?.Name ?? "N/A").FontSize(9).FontColor("#cccccc");
                                    row.RelativeItem(1).AlignCenter().Text(reservation.Seats.Count.ToString()).FontSize(9).FontColor("#cccccc");
                                    row.RelativeItem(1).AlignRight().Text($"{reservation.TotalPrice:F2}€").FontSize(9).FontColor("#10b981");
                                });
                        }
                    });

                    page.Footer().AlignCenter().Text($"Cinema Rocha - Relatório de Reservas").FontSize(10).FontColor("#666666");
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Relatorio_Reservas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s!.Movie)
                .Include(r => r.Session)
                    .ThenInclude(s => s!.Room)
                .Include(r => r.Seats)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        public async Task<IActionResult> Movies()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public IActionResult CreateMovie()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovie([Bind("Id,Title,Description,Genre,Duration,ImageUrl,TrailerUrl,LogoUrl,BackdropUrl,ReleaseDate,ImdbRating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Movies));
            }
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> EditMovie(int? id)
        {
            if (id == null) return NotFound();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(int id, [Bind("Id,Title,Description,Genre,Duration,ImageUrl,TrailerUrl,LogoUrl,BackdropUrl,ReleaseDate,ImdbRating")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Movies));
            }
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteMovie(int? id)
        {
            if (id == null) return NotFound();
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Movies));
        }

        [HttpGet]
        public async Task<IActionResult> SearchTmdb(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var results = await _tmdbService.SearchMoviesAsync(query);
            return Json(results.Select(m => new
            {
                id = m.Id,
                title = m.Title,
                overview = m.Overview,
                posterPath = m.PosterPath,
                releaseDate = m.ReleaseDate,
                voteAverage = m.VoteAverage
            }));
        }

        [HttpGet]
        public async Task<IActionResult> GetTmdbMovie(int id)
        {
            var movie = await _tmdbService.GetMovieDetailsAsync(id);
            if (movie == null)
                return NotFound();

            return Json(new
            {
                title = movie.Title,
                description = movie.Overview,
                genre = string.Join(", ", movie.Genres),
                duration = movie.Runtime,
                imageUrl = movie.PosterPath,
                releaseDate = movie.ReleaseDate,
                imdbRating = movie.ImdbRating
            });
        }

        [HttpPost]
        public async Task<IActionResult> PopulateLogos()
        {
            try
            {
                var movies = await _context.Movies.Where(m => string.IsNullOrEmpty(m.LogoUrl)).ToListAsync();
                int updated = 0;

                foreach (var movie in movies)
                {
                    try
                    {
                        var searchResults = await _tmdbService.SearchMoviesAsync(movie.Title);
                        var tmdbMovie = searchResults.FirstOrDefault();
                        
                        if (tmdbMovie != null)
                        {
                            var logoUrl = await _tmdbService.GetMovieLogoAsync(tmdbMovie.Id);
                            if (!string.IsNullOrEmpty(logoUrl))
                            {
                                movie.LogoUrl = logoUrl;
                                updated++;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (updated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, updated = updated, total = movies.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PopulateRatings()
        {
            try
            {
                var movies = await _context.Movies.ToListAsync();
                int updated = 0;

                foreach (var movie in movies)
                {
                    try
                    {
                        Console.WriteLine($"[PopulateRatings] Processing: {movie.Title}");
                        var searchResults = await _tmdbService.SearchMoviesAsync(movie.Title);
                        var tmdbMovie = searchResults.FirstOrDefault();
                        
                        if (tmdbMovie != null)
                        {
                            Console.WriteLine($"[PopulateRatings] Found on TMDB: {tmdbMovie.Title} (ID: {tmdbMovie.Id})");
                            var details = await _tmdbService.GetMovieDetailsAsync(tmdbMovie.Id);
                            if (details != null && !string.IsNullOrEmpty(details.ImdbRating))
                            {
                                Console.WriteLine($"[PopulateRatings] Found Rating: {details.ImdbRating}");
                                movie.ImdbRating = details.ImdbRating;
                                updated++;
                            }
                            else
                            {
                                Console.WriteLine($"[PopulateRatings] No rating or details found for {movie.Title} (Imdb_id: {details?.Imdb_id ?? "null"})");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PopulateRatings] Movie not found on TMDB: {movie.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PopulateRatings] Error for {movie.Title}: {ex.Message}");
                        continue;
                    }
                }

                if (updated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, updated = updated, total = movies.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Sessions()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .ToListAsync();
            return View(sessions);
        }

        public IActionResult CreateSession()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession([Bind("Id,MovieId,RoomId,StartTime,Price")] Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Sessions));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name", session.RoomId);
            return View(session);
        }

        public async Task<IActionResult> EditSession(int? id)
        {
            if (id == null) return NotFound();
            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name", session.RoomId);
            return View(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(int id, [Bind("Id,MovieId,RoomId,StartTime,Price")] Session session)
        {
            if (id != session.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Sessions));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name", session.RoomId);
            return View(session);
        }

        public async Task<IActionResult> DeleteSession(int? id)
        {
            if (id == null) return NotFound();
            
            var session = await _context.Sessions
                .Include(s => s.Reservations)
                .ThenInclude(r => r.Seats)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (session != null)
            {
                foreach (var reservation in session.Reservations)
                {
                    foreach (var seat in reservation.Seats)
                    {
                        seat.ReservationId = null;
                    }
                }
                await _context.SaveChangesAsync();
                
                _context.Reservations.RemoveRange(session.Reservations);
                await _context.SaveChangesAsync();
                
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Sessions));
        }

        public async Task<IActionResult> Rooms()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        public IActionResult CreateRoom()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom([Bind("Id,Name,Capacity")] Room room)
        {
            if (ModelState.IsValid)
            {
                int rows = (int)Math.Ceiling((double)room.Capacity / 10);
                var seats = new List<Seat>();
                
                for (int i = 0; i < rows; i++)
                {
                    char rowChar = (char)('A' + i);
                    int seatsInThisRow = Math.Min(10, room.Capacity - (i * 10));
                    
                    for (int j = 1; j <= seatsInThisRow; j++)
                    {
                        seats.Add(new Seat { Row = rowChar.ToString(), Number = j, Room = room });
                    }
                }
                
                room.Seats = seats;
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Rooms));
            }
            return View(room);
        }

        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, [Bind("Id,Name,Capacity")] Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Rooms));
            }
            return View(room);
        }

        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Rooms));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .Include(c => c.UserCoupons)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(coupons);
        }

        public IActionResult CreateCoupon()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon([Bind("Code,Type,DiscountValue,ValidFrom,ValidTo,MaxUses,Description")] Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.CreatedBy = User.Identity?.Name;
                coupon.CreatedAt = DateTime.Now;
                coupon.IsActive = true;
                coupon.CurrentUses = 0;
                
                _context.Add(coupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Coupons));
            }
            return View(coupon);
        }

        public async Task<IActionResult> EditCoupon(int? id)
        {
            if (id == null) return NotFound();
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(int id, [Bind("Id,Code,Type,DiscountValue,ValidFrom,ValidTo,MaxUses,IsActive,Description")] Coupon coupon)
        {
            if (id != coupon.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Coupons.FindAsync(id);
                    if (existing == null) return NotFound();
                    
                    existing.Code = coupon.Code;
                    existing.Type = coupon.Type;
                    existing.DiscountValue = coupon.DiscountValue;
                    existing.ValidFrom = coupon.ValidFrom;
                    existing.ValidTo = coupon.ValidTo;
                    existing.MaxUses = coupon.MaxUses;
                    existing.IsActive = coupon.IsActive;
                    existing.Description = coupon.Description;
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Coupons));
            }
            return View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCoupon(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Coupons));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSessionsWithAI([FromBody] AISessionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Prompt))
                {
                    return Json(new { success = false, message = "Por favor, descreve as sessões que queres criar." });
                }

                var result = await _geminiService.GenerateSessionsAsync(request.Prompt);
                return Json(new { success = result.Success, sessions = result.Sessions, message = result.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Generation Error: {ex.Message}");
                return Json(new { success = false, message = $"Erro no servidor: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAISessions([FromBody] CreateAISessionsRequest request)
        {
            int created = 0;
            int updated = 0;
            int deleted = 0;
            int errors = 0;

            foreach (var s in request.Sessions)
            {
                try
                {
                    if (s.Action == "CREATE")
                    {
                        var dateTime = DateTime.Parse($"{s.Date} {s.Time}");
                        var newSession = new Session
                        {
                            MovieId = s.MovieId,
                            RoomId = s.RoomId,
                            StartTime = dateTime,
                            Price = s.Price
                        };
                        _context.Sessions.Add(newSession);
                        created++;
                    }
                    else if (s.Action == "EDIT" && s.Id.HasValue)
                    {
                        var existing = await _context.Sessions.FindAsync(s.Id.Value);
                        if (existing != null)
                        {
                            var dateTime = DateTime.Parse($"{s.Date} {s.Time}");
                            if (s.MovieId > 0) existing.MovieId = s.MovieId;
                            if (s.RoomId > 0) existing.RoomId = s.RoomId;
                            existing.StartTime = dateTime;
                            existing.Price = s.Price;
                            _context.Sessions.Update(existing);
                            updated++;
                        }
                    }
                    else if (s.Action == "DELETE" && s.Id.HasValue)
                    {
                        var existing = await _context.Sessions
                            .Include(sess => sess.Reservations)
                            .FirstOrDefaultAsync(sess => sess.Id == s.Id.Value);

                        if (existing != null)
                        {
                            if (existing.Reservations != null && existing.Reservations.Any())
                            {
                                errors++;
                                continue;
                            }
                            _context.Sessions.Remove(existing);
                            deleted++;
                        }
                    }
                }
                catch { errors++; }
            }

            await _context.SaveChangesAsync();
            return Json(new { 
                success = true, 
                created = created, 
                updated = updated, 
                deleted = deleted,
                errors = errors
            });
        }
    }

    public class AISessionRequest
    {
        public string Prompt { get; set; } = "";
    }

    public class CreateAISessionsRequest
    {
        public List<GeneratedSession> Sessions { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public int TotalMovies { get; set; }
        public int TotalSessions { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Reservation> RecentReservations { get; set; } = new List<Reservation>();
        public List<Movie> RecentMovies { get; set; } = new List<Movie>();
        public List<Session> RecentSessions { get; set; } = new List<Session>();
        public List<Room> RecentRooms { get; set; } = new List<Room>();
    }
}
