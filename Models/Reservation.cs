using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CinemaRocha.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User is required")]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Session is required")]
        [ForeignKey("Session")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDate { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public IdentityUser User { get; set; } = null!;
        public Session Session { get; set; } = null!;
    }
}
