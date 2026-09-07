using System.ComponentModel.DataAnnotations;

namespace RochaCinema.Models;

public class Ticket
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    [Required]
    public int ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;
    
    [Required(ErrorMessage = "O número do assento é obrigatório")]
    [StringLength(10)]
    public string SeatNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime PurchaseDate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Ativo"; // Ativo, Cancelado
    
    public decimal PricePaid { get; set; }
}
