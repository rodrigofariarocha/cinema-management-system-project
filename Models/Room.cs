using System.ComponentModel.DataAnnotations;

namespace CinemaRocha.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        // Navigation property
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
