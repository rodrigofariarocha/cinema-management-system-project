using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaRocha.Models
{
    public class Seat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Row { get; set; } = string.Empty; 

        public int Number { get; set; } 

        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }
    }
}
