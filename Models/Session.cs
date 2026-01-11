using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaRocha.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Movie is required")]
        [ForeignKey("Movie")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 1000.00, ErrorMessage = "Price must be between 0.01 and 1000.00")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public Movie? Movie { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
