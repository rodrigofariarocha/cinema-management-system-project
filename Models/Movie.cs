using System.ComponentModel.DataAnnotations;

namespace RochaCinema.Models;

public class Movie
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A duração é obrigatória")]
    [Range(1, 500, ErrorMessage = "A duração deve estar entre 1 e 500 minutos")]
    public int DurationMinutes { get; set; }
    
    [Required(ErrorMessage = "O género é obrigatório")]
    [StringLength(100)]
    public string Genre { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? PosterUrl { get; set; }
    
    [Required(ErrorMessage = "A data de lançamento é obrigatória")]
    public DateTime ReleaseDate { get; set; }
    
    [StringLength(100)]
    public string? Director { get; set; }
    
    [Range(0, 10, ErrorMessage = "A classificação deve estar entre 0 e 10")]
    public decimal? Rating { get; set; }
    
    // Navigation property
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
