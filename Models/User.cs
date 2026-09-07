using Microsoft.AspNetCore.Identity;

namespace RochaCinema.Models;

public class User : IdentityUser
{
    public string? FullName { get; set; }
    
    // Navigation property
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
