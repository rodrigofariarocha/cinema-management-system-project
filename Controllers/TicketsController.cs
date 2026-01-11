using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Data;
using CinemaRocha.Models;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace CinemaRocha.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Room)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Reservations)
                    .ThenInclude(r => r.Seats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int sessionId, string selectedSeats, string? couponCode)
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

            decimal totalPrice = seats.Count * session.Price;
            UserCoupon? appliedCoupon = null;
            
            if (!string.IsNullOrEmpty(couponCode))
            {
                appliedCoupon = await _context.UserCoupons
                    .Include(uc => uc.Coupon)
                    .FirstOrDefaultAsync(uc => 
                        uc.UserId == userId && 
                        uc.Coupon.Code == couponCode && 
                        !uc.IsUsed && 
                        uc.Coupon.IsActive &&
                        uc.Coupon.ValidTo >= DateTime.Now);
                
                if (appliedCoupon != null && appliedCoupon.Coupon.Type == CouponType.FreeTicket)
                {
                    totalPrice = Math.Max(0, (seats.Count - 1) * session.Price);
                }
            }

            var reservation = new Reservation
            {
                UserId = userId,
                SessionId = sessionId,
                ReservationDate = DateTime.Now,
                TotalPrice = totalPrice
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            foreach (var seat in seats)
            {
                seat.ReservationId = reservation.Id;
            }
            
            if (appliedCoupon != null)
            {
                appliedCoupon.IsUsed = true;
                appliedCoupon.UsedAt = DateTime.Now;
                appliedCoupon.ReservationId = reservation.Id;
            }
            
            await _context.SaveChangesAsync();

            if (appliedCoupon == null)
            {
                await AddLoyaltyPoints(userId, seats.Count);
            }

            return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Code))
            {
                return Json(new { success = false, message = "Código inválido" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userCoupon = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .FirstOrDefaultAsync(uc => 
                    uc.UserId == userId && 
                    uc.Coupon.Code == request.Code && 
                    !uc.IsUsed && 
                    uc.Coupon.IsActive &&
                    uc.Coupon.ValidTo >= DateTime.Now);

            if (userCoupon == null)
            {
                return Json(new { success = false, message = "Código inválido ou já utilizado" });
            }

            if (userCoupon.Coupon.Type == CouponType.FreeTicket)
            {
                return Json(new { success = true, isFree = true, seatLimit = 1, message = "1 bilhete grátis! Seleciona apenas 1 lugar." });
            }

            return Json(new { success = false, message = "Tipo de cupão não suportado" });
        }
        
        [HttpPost]
        [Authorize]
        [Route("/Loyalty/GenerateCode")]
        public async Task<IActionResult> GenerateCouponCode([FromBody] GenerateCodeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var userCoupon = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .FirstOrDefaultAsync(uc => uc.Id == request.UserCouponId && uc.UserId == userId && !uc.IsUsed);
            
            if (userCoupon == null)
            {
                return Json(new { success = false, message = "Cupão não encontrado" });
            }
            
            if (userCoupon.Coupon.Code == "PENDING" || userCoupon.Coupon.Code.StartsWith("PENDING"))
            {
                var newCode = GenerateNumericCode();
                userCoupon.Coupon.Code = newCode;
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true, code = userCoupon.Coupon.Code });
        }

        private async Task AddLoyaltyPoints(string userId, int ticketCount)
        {
            var loyalty = await _context.UserLoyalties.FirstOrDefaultAsync(ul => ul.UserId == userId);
            
            if (loyalty == null)
            {
                loyalty = new UserLoyalty { UserId = userId };
                _context.UserLoyalties.Add(loyalty);
            }

            loyalty.AvailablePoints += ticketCount;
            loyalty.TotalPoints += ticketCount;
            loyalty.LastUpdated = DateTime.Now;

            while (loyalty.AvailablePoints >= 10)
            {
                loyalty.AvailablePoints -= 10;

                var pendingId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                var coupon = new Coupon
                {
                    Code = $"PENDING-{pendingId}",
                    Type = CouponType.FreeTicket,
                    DiscountValue = 100,
                    ValidFrom = DateTime.Now,
                    ValidTo = DateTime.Now.AddMonths(3),
                    MaxUses = 1,
                    IsActive = true,
                    CreatedBy = "SYSTEM",
                    Description = "1 bilhete grátis por fidelidade - parabéns!"
                };
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                var userCoupon = new UserCoupon
                {
                    UserId = userId,
                    CouponId = coupon.Id,
                    IssuedAt = DateTime.Now
                };
                
                _context.UserCoupons.Add(userCoupon);
            }

            await _context.SaveChangesAsync();
        }

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

        [Authorize]
        public async Task<IActionResult> DownloadTicket(int? id)
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

            var pdfBytes = await GenerateTicketPdf(reservation);
            return File(pdfBytes, "application/pdf", $"Bilhete-CinemaRocha-{reservation.Id:D6}.pdf");
        }

        private async Task<byte[]> GenerateTicketPdf(Reservation reservation)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var movie = reservation.Session.Movie;
            var session = reservation.Session;
            var seats = string.Join(", ", reservation.Seats.Select(s => $"{s.Row}{s.Number}"));
            var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=ROCHA-{reservation.Id}";

            byte[] posterImage = null;
            byte[] qrImage = null;

            using (var client = new HttpClient())
            {
                try
                {
                    if (!string.IsNullOrEmpty(movie.ImageUrl))
                        posterImage = await client.GetByteArrayAsync(movie.ImageUrl);
                    
                    qrImage = await client.GetByteArrayAsync(qrCodeUrl);
                }
                catch
                {
                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(0);
                    page.PageColor("#0a0a0a");

                    page.Content().Row(row =>
                    {
                        row.ConstantItem(250).Image(posterImage).FitArea();

                        row.RelativeItem().Padding(30).Column(col =>
                        {
                            col.Item().Text(movie.Title).FontSize(28).Bold().FontColor("#ffffff").LineHeight(1.1f);
                            
                            col.Item().PaddingTop(10).Row(meta =>
                            {
                                meta.AutoItem().Text(movie.Genre).FontSize(14).FontColor("#aaaaaa");
                                meta.ConstantItem(20).AlignCenter().Text("•").FontSize(14).FontColor("#aaaaaa");
                                meta.AutoItem().Text($"{movie.Duration} min").FontSize(14).FontColor("#aaaaaa");
                            });

                            col.Item().PaddingTop(20).Container().Background("#ff3b30").PaddingVertical(5).PaddingHorizontal(15).CornerRadius(5).Text(session.Room.Name).SemiBold().FontColor("#ffffff");

                            col.Item().PaddingTop(30).Row(stats => 
                            {
                                stats.RelativeItem().Column(c => {
                                    c.Item().Text("DATA").FontSize(10).FontColor("#666666").SemiBold();
                                    c.Item().Text(session.StartTime.ToString("dd MMM yyyy")).FontSize(16).Bold().FontColor("#ffffff");
                                });
                                
                                stats.RelativeItem().Column(c => {
                                    c.Item().Text("HORA").FontSize(10).FontColor("#666666").SemiBold();
                                    c.Item().Text(session.StartTime.ToString("HH:mm")).FontSize(16).Bold().FontColor("#ffffff");
                                });

                                stats.RelativeItem().Column(c => {
                                    c.Item().Text("BILHETES").FontSize(10).FontColor("#666666").SemiBold();
                                    c.Item().Text(reservation.Seats.Count.ToString()).FontSize(16).Bold().FontColor("#ffffff");
                                });
                            });

                            col.Item().PaddingTop(20).Column(c => {
                                c.Item().PaddingBottom(5).Text("LUGARES RESERVADOS").FontSize(10).FontColor("#666666").SemiBold();
                                c.Item().Row(r => {
                                    foreach(var seat in reservation.Seats)
                                    {
                                        r.AutoItem().PaddingRight(10).Container()
                                            .Border(1).BorderColor("#ff3b30")
                                            .Background("#1a1a1a")
                                            .PaddingVertical(5).PaddingHorizontal(10)
                                            .CornerRadius(5)
                                            .Text($"{seat.Row}{seat.Number}").Bold().FontColor("#ff3b30");
                                    }
                                });
                            });

                            col.Item().PaddingTop(30).LineHorizontal(1).LineColor("#333333");

                            col.Item().PaddingTop(20).Row(footer =>
                            {
                                if(qrImage != null)
                                    footer.ConstantItem(80).Image(qrImage);
                                
                                footer.RelativeItem().PaddingLeft(20).Column(c => {
                                    c.Item().Text("Apresenta este código à entrada").FontSize(10).FontColor("#666666");
                                    c.Item().PaddingTop(5).Container().Background("#1a1a1a").Padding(10).CornerRadius(5)
                                        .Text($"Ref: ROCHA-{reservation.Id:D6}").FontFamily("Courier New").FontColor("#ff3b30").SemiBold();
                                });
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string GenerateQRCode(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrCodeData))
                {
                    using (var bitmap = qrCode.GetGraphic(20))
                    {
                        using (var stream = new MemoryStream())
                        {
                            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return Convert.ToBase64String(stream.ToArray());
                        }
                    }
                }
            }
        }
        
        private string GenerateNumericCode()
        {
            var random = new Random();
            var code = "";
            for (int i = 0; i < 8; i++)
            {
                code += random.Next(0, 10).ToString();
            }
            return code;
        }
    }
    
    public class CouponValidationRequest
    {
        public string? Code { get; set; }
    }
    
    public class GenerateCodeRequest
    {
        public int UserCouponId { get; set; }
    }
}
