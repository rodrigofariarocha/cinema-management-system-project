using System.ComponentModel.DataAnnotations;

namespace CinemaRocha.Models
{
    public enum RoomType
    {
        Small,      // 4-5 rows, ~40 seats
        Normal1,    // 6-7 rows, ~90 seats  
        Normal2,    // 7-8 rows, ~120 seats
        Large       // 10+ rows, ~180 seats (IMAX)
    }

    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public RoomType Type { get; set; }

        [StringLength(200)]
        public string Features { get; set; } = string.Empty; // e.g., "IMAX, Dolby Atmos, Reclinable Seats"

        // Navigation property
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
