using System.Text.Json;

namespace CinemaRocha.Services
{
    public class TmdbMovie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? Poster_path { get; set; }
        public string? Release_date { get; set; }
        public List<TmdbGenre> Genres { get; set; } = new();
        public int Runtime { get; set; }
        public double Vote_average { get; set; }
    }

    public class TmdbGenre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TmdbSearchResult
    {
        public int Page { get; set; }
        public List<TmdbSearchMovie> Results { get; set; } = new();
    }

    public class TmdbSearchMovie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Overview { get; set; }
        public string? Poster_path { get; set; }
        public string? Release_date { get; set; }
        public double Vote_average { get; set; }
    }

    public class TmdbMovieDetails
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public string? ReleaseDate { get; set; }
        public int Runtime { get; set; }
        public double VoteAverage { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? LogoPath { get; set; }
    }

    public class TmdbImages
    {
        public List<TmdbLogo> Logos { get; set; } = new();
    }

    public class TmdbLogo
    {
        public double Aspect_ratio { get; set; }
        public int Height { get; set; }
        public string? Iso_639_1 { get; set; }
        public string File_path { get; set; } = string.Empty;
        public double Vote_average { get; set; }
        public int Vote_count { get; set; }
        public int Width { get; set; }
    }

    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _bearerToken;

        public TmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Tmdb:ApiKey"] ?? throw new InvalidOperationException("TMDB API Key not configured");
            _bearerToken = configuration["Tmdb:BearerToken"] ?? "";
            
            var baseUrl = configuration["Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3";
            _httpClient.BaseAddress = new Uri(baseUrl);
            
            // Set Bearer token if available, otherwise use API key
            if (!string.IsNullOrEmpty(_bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
            }
        }

        public async Task<List<TmdbMovieDetails>> SearchMoviesAsync(string query)
        {
            try
            {
                // Use Bearer token if available, otherwise fall back to API key
                var url = !string.IsNullOrEmpty(_bearerToken)
                    ? $"/3/search/movie?query={Uri.EscapeDataString(query)}&language=pt-PT"
                    : $"/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language=pt-PT";
                    
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TmdbSearchResult>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Convert search results to details format with proper image URLs
                return result?.Results.Select(r => new TmdbMovieDetails
                {
                    Id = r.Id,
                    Title = r.Title ?? "Unknown",
                    Overview = r.Overview ?? "",
                    ReleaseDate = r.Release_date,
                    VoteAverage = r.Vote_average,
                    PosterPath = !string.IsNullOrEmpty(r.Poster_path) 
                        ? $"https://image.tmdb.org/t/p/original{r.Poster_path}"
                        : null
                }).ToList() ?? new List<TmdbMovieDetails>();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"TMDB Search Error: {ex.Message}");
                return new List<TmdbMovieDetails>();
            }
        }

        public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId)
        {
            try
            {
                // Use Bearer token if available, otherwise fall back to API key
                var url = !string.IsNullOrEmpty(_bearerToken)
                    ? $"/3/movie/{tmdbId}?language=pt-PT"
                    : $"/3/movie/{tmdbId}?api_key={_apiKey}&language=pt-PT";
                    
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var movie = JsonSerializer.Deserialize<TmdbMovie>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (movie == null) return null;

                // Fetch logo
                var logoUrl = await GetMovieLogoAsync(tmdbId);

                return new TmdbMovieDetails
                {
                    Id = movie.Id,
                    Title = movie.Title ?? "Unknown",
                    Overview = movie.Overview ?? "",
                    ReleaseDate = movie.Release_date,
                    Runtime = movie.Runtime,
                    VoteAverage = movie.Vote_average,
                    PosterPath = !string.IsNullOrEmpty(movie.Poster_path)
                        ? $"https://image.tmdb.org/t/p/original{movie.Poster_path}"
                        : null,
                    Genres = movie.Genres?.Select(g => g.Name ?? "").ToList() ?? new List<string>(),
                    LogoPath = logoUrl
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TMDB Get Movie Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetMovieLogoAsync(int tmdbId)
        {
            try
            {
                // Fetch images for the movie
                var url = !string.IsNullOrEmpty(_bearerToken)
                    ? $"/3/movie/{tmdbId}/images"
                    : $"/3/movie/{tmdbId}/images?api_key={_apiKey}";
                    
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var images = JsonSerializer.Deserialize<TmdbImages>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (images?.Logos == null || !images.Logos.Any())
                    return null;

                // Prefer Portuguese logos, fall back to English, then any language
                var logo = images.Logos
                    .Where(l => l.Iso_639_1 == "pt")
                    .OrderByDescending(l => l.Vote_average)
                    .ThenByDescending(l => l.Width)
                    .FirstOrDefault()
                    ?? images.Logos
                        .Where(l => l.Iso_639_1 == "en")
                        .OrderByDescending(l => l.Vote_average)
                        .ThenByDescending(l => l.Width)
                        .FirstOrDefault()
                    ?? images.Logos
                        .OrderByDescending(l => l.Vote_average)
                        .ThenByDescending(l => l.Width)
                        .FirstOrDefault();

                if (logo != null && !string.IsNullOrEmpty(logo.File_path))
                {
                    return $"https://image.tmdb.org/t/p/original{logo.File_path}";
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TMDB Get Logo Error: {ex.Message}");
                return null;
            }
        }
    }
}
