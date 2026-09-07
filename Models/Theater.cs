using System.ComponentModel.DataAnnotations;

namespace RochaCinema.Models;

public class Theater
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O nome da sala é obrigatório")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A capacidade é obrigatória")]
    [Range(1, 1000, ErrorMessage = "A capacidade deve estar entre 1 e 1000")]
    public int Capacity { get; set; }
    
    // Navigation property
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
