using System.ComponentModel.DataAnnotations;

namespace CinemaRocha.Models
{
    public enum RoomType
    {
        Small,
        Normal1,
        Normal2,
        Large
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
        public string Features { get; set; } = string.Empty;

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
