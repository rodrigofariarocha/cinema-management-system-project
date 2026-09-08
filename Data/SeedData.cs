using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CinemaRocha.Models;

namespace CinemaRocha.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Cria/atualiza o esquema da base de dados no arranque
                context.Database.Migrate();

                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var adminEmail = "admin@rochacinema.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(adminUser, "Admin123!");
                }

                if (!context.Movies.Any())
                {
                    context.Movies.AddRange(
                    new Movie
                    {
                        Title = "The Great Adventure",
                        Description = "Uma jornada épica através de territórios desconhecidos.",
                        Genre = "Aventura",
                        Duration = 142,
                        ImageUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=400&h=600&fit=crop",
                        BackdropUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=1920&h=800&fit=crop",
                        TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        ReleaseDate = DateTime.Now.AddMonths(-6)
                    },
                    new Movie
                    {
                        Title = "Mystery at Midnight",
                        Description = "Um detetive corre contra o tempo para resolver o caso.",
                        Genre = "Thriller",
                        Duration = 128,
                        ImageUrl = "https://images.unsplash.com/photo-1594908900066-3f47337549d8?w=400&h=600&fit=crop",
                        BackdropUrl = "https://images.unsplash.com/photo-1594908900066-3f47337549d8?w=1920&h=800&fit=crop",
                        TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        ReleaseDate = DateTime.Now.AddMonths(-4)
                    },
                    new Movie
                    {
                        Title = "Love & Dreams",
                        Description = "Uma história de amor que desafia o destino.",
                        Genre = "Romance",
                        Duration = 115,
                        ImageUrl = "https://images.unsplash.com/photo-1598899134739-24c46f58b8c0?w=400&h=600&fit=crop",
                        BackdropUrl = "https://images.unsplash.com/photo-1598899134739-24c46f58b8c0?w=1920&h=800&fit=crop",
                        TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        ReleaseDate = DateTime.Now.AddMonths(-2)
                    },
                    new Movie
                    {
                        Title = "Comedy Night",
                        Description = "Prepare-se para rir até chorar com este grupo de amigos.",
                        Genre = "Comédia",
                        Duration = 98,
                        ImageUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=400&h=600&fit=crop",
                        BackdropUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=1920&h=800&fit=crop",
                        TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        ReleaseDate = DateTime.Now.AddMonths(-1)
                    },
                    new Movie
                    {
                        Title = "Beyond the Stars",
                        Description = "A humanidade procura um novo lar nas estrelas.",
                        Genre = "Sci-Fi",
                        Duration = 156,
                        ImageUrl = "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=400&h=600&fit=crop",
                        BackdropUrl = "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=1920&h=800&fit=crop",
                        TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        ReleaseDate = DateTime.Now
                    }
                );
                await context.SaveChangesAsync();
                }

                if (!context.Rooms.Any())
                {
                    var roomSmall = new Room 
                    { 
                        Name = "Sala 1 (Pequena)", 
                        Capacity = 48,
                        Type = RoomType.Small,
                        Features = "Standard Sound, Comfortable Seats"
                    };
                    
                    var roomNormal1 = new Room 
                    { 
                        Name = "Sala 2 (Normal)", 
                        Capacity = 84,
                        Type = RoomType.Normal1,
                        Features = "Dolby Digital, Comfortable Seats"
                    };
                    
                    var roomNormal2 = new Room 
                    { 
                        Name = "Sala 3 (Normal Plus)", 
                        Capacity = 128,
                        Type = RoomType.Normal2,
                        Features = "Atmos Sound, Extra Legroom"
                    };
                    
                    var roomLarge = new Room 
                    { 
                        Name = "Sala 4 (IMAX)", 
                        Capacity = 180,
                        Type = RoomType.Large,
                        Features = "IMAX Screen, Dolby Atmos, Luxury Reclinable Seats"
                    };
                    
                    context.Rooms.AddRange(roomSmall, roomNormal1, roomNormal2, roomLarge);
                    await context.SaveChangesAsync();

                    void CreateSeatsForRoom(int roomId, int rows, int seatsPerRow, string[] rowLetters)
                    {
                        var seats = new List<Seat>();
                        for (int r = 0; r < rows; r++)
                        {
                            for (int s = 1; s <= seatsPerRow; s++)
                            {
                                seats.Add(new Seat 
                                { 
                                    Row = rowLetters[r], 
                                    Number = s, 
                                    RoomId = roomId 
                                });
                            }
                        }
                        context.Seats.AddRange(seats);
                    }

                    CreateSeatsForRoom(roomSmall.Id, 4, 12, new[] { "A", "B", "C", "D" });
                    
                    CreateSeatsForRoom(roomNormal1.Id, 6, 14, new[] { "A", "B", "C", "D", "E", "F" });
                    
                    CreateSeatsForRoom(roomNormal2.Id, 8, 16, new[] { "A", "B", "C", "D", "E", "F", "G", "H" });
                    
                    CreateSeatsForRoom(roomLarge.Id, 10, 18, new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" });

                    await context.SaveChangesAsync();
                }

                if (!context.Sessions.Any())
                {
                    var movies = await context.Movies.ToListAsync();
                    var rooms = await context.Rooms.ToListAsync();
                    
                    if (movies.Any() && rooms.Any())
                    {
                        var today = DateTime.Today;
                        var sessions = new List<Session>();

                        for (int i = 0; i < 7; i++)
                        {
                            var date = today.AddDays(i);
                            
                            if (movies.Count > 0 && rooms.Count > 0)
                            {
                                sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(14).AddMinutes(30), Price = 12.50m });
                                sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(18).AddMinutes(00), Price = 12.50m });
                                sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(21).AddMinutes(30), Price = 12.50m });
                            }
                            
                            if (movies.Count > 1 && rooms.Count > 1)
                            {
                                sessions.Add(new Session { MovieId = movies[1].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(15).AddMinutes(00), Price = 8.50m });
                                sessions.Add(new Session { MovieId = movies[1].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(19).AddMinutes(00), Price = 8.50m });
                            }
                            
                            if (movies.Count > 2 && rooms.Count > 1)
                            {
                                sessions.Add(new Session { MovieId = movies[2].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(17).AddMinutes(00), Price = 8.50m });
                                sessions.Add(new Session { MovieId = movies[2].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(21).AddMinutes(00), Price = 8.50m });
                            }
                        }

                        if (sessions.Any())
                        {
                            context.Sessions.AddRange(sessions);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}
