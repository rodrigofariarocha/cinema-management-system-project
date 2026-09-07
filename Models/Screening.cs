using System.ComponentModel.DataAnnotations;

namespace RochaCinema.Models;

public class Screening
{
    public int Id { get; set; }
    
    [Required]
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    
    [Required]
    public int TheaterId { get; set; }
    public Theater Theater { get; set; } = null!;
    
    [Required(ErrorMessage = "A data e hora são obrigatórias")]
    public DateTime DateTime { get; set; }
    
    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, 1000, ErrorMessage = "O preço deve ser maior que 0")]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int AvailableSeats { get; set; }
    
    // Navigation property
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
