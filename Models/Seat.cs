using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaRocha.Models
{
    public class Seat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Row { get; set; } = string.Empty; // e.g., "A", "B"

        public int Number { get; set; } // e.g., 1, 2, 3

        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
    }
}
