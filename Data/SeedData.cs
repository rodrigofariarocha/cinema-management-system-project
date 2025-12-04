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
                // Ensure database is created
                context.Database.EnsureCreated();

                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // 1. Create Admin User
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

                // 2. Seed Movies if empty
                if (!context.Movies.Any())
                {
                    context.Movies.AddRange(
                        new Movie
                        {
                            Title = "The Great Adventure",
                            Description = "Uma jornada épica através de territórios desconhecidos.",
                            Genre = "Aventura",
                            Duration = 142,
                            ImageUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=400&h=600&fit=crop"
                        },
                        new Movie
                        {
                            Title = "Mystery at Midnight",
                            Description = "Um detetive corre contra o tempo para resolver o caso.",
                            Genre = "Thriller",
                            Duration = 128,
                            ImageUrl = "https://images.unsplash.com/photo-1594908900066-3f47337549d8?w=400&h=600&fit=crop"
                        },
                        new Movie
                        {
                            Title = "Love & Dreams",
                            Description = "Uma história de amor que desafia o destino.",
                            Genre = "Romance",
                            Duration = 115,
                            ImageUrl = "https://images.unsplash.com/photo-1598899134739-24c46f58b8c0?w=400&h=600&fit=crop"
                        },
                        new Movie
                        {
                            Title = "Comedy Night",
                            Description = "Prepare-se para rir até chorar com este grupo de amigos.",
                            Genre = "Comédia",
                            Duration = 98,
                            ImageUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=400&h=600&fit=crop"
                        },
                        new Movie
                        {
                            Title = "Beyond the Stars",
                            Description = "A humanidade procura um novo lar nas estrelas.",
                            Genre = "Sci-Fi",
                            Duration = 156,
                            ImageUrl = "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=400&h=600&fit=crop"
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // 3. Seed Rooms if empty
                if (!context.Rooms.Any())
                {
                    var room1 = new Room { Name = "Sala 1 (IMAX)", Capacity = 50 };
                    var room2 = new Room { Name = "Sala 2 (Standard)", Capacity = 40 };
                    
                    context.Rooms.AddRange(room1, room2);
                    await context.SaveChangesAsync();

                    // Add Seats to Room 1 (5 rows x 10 seats)
                    var seats1 = new List<Seat>();
                    string[] rows = { "A", "B", "C", "D", "E" };
                    foreach (var row in rows)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            seats1.Add(new Seat { Row = row, Number = i, RoomId = room1.Id });
                        }
                    }
                    context.Seats.AddRange(seats1);

                    // Add Seats to Room 2 (4 rows x 10 seats)
                    var seats2 = new List<Seat>();
                    string[] rows2 = { "A", "B", "C", "D" };
                    foreach (var row in rows2)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            seats2.Add(new Seat { Row = row, Number = i, RoomId = room2.Id });
                        }
                    }
                    context.Seats.AddRange(seats2);
                    await context.SaveChangesAsync();
                }

                // 4. Seed Sessions if empty
                if (!context.Sessions.Any())
                {
                    var movies = await context.Movies.ToListAsync();
                    var rooms = await context.Rooms.ToListAsync();
                    
                    if (movies.Any() && rooms.Any())
                    {
                        var today = DateTime.Today;
                        var sessions = new List<Session>();

                        // Sessions for today and tomorrow
                        for (int i = 0; i < 2; i++)
                        {
                            var date = today.AddDays(i);
                            
                            // Movie 1 in Room 1
                            sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(14).AddMinutes(30), Price = 12.50m });
                            sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(18).AddMinutes(00), Price = 12.50m });
                            sessions.Add(new Session { MovieId = movies[0].Id, RoomId = rooms[0].Id, StartTime = date.AddHours(21).AddMinutes(30), Price = 12.50m });

                            // Movie 2 in Room 2
                            sessions.Add(new Session { MovieId = movies[1].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(15).AddMinutes(00), Price = 8.50m });
                            sessions.Add(new Session { MovieId = movies[1].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(19).AddMinutes(00), Price = 8.50m });

                            // Movie 3 in Room 2
                            sessions.Add(new Session { MovieId = movies[2].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(17).AddMinutes(00), Price = 8.50m });
                            sessions.Add(new Session { MovieId = movies[2].Id, RoomId = rooms[1].Id, StartTime = date.AddHours(21).AddMinutes(00), Price = 8.50m });
                        }

                        context.Sessions.AddRange(sessions);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
