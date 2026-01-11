using System.Text;
using System.Text.Json;
using CinemaRocha.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaRocha.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public GeminiService(IConfiguration configuration, ApplicationDbContext context)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API key not configured");
            _httpClient = new HttpClient();
            _context = context;
        }

        public async Task<GeminiSessionResponse> GenerateSessionsAsync(string userPrompt)
        {
            var movies = await _context.Movies.Select(m => new { m.Id, m.Title }).ToListAsync();
            var rooms = await _context.Rooms.Select(r => new { r.Id, r.Name }).ToListAsync();
            var sessions = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .Where(s => s.StartTime >= DateTime.Today)
                .OrderBy(s => s.StartTime)
                .Select(s => new { 
                    s.Id, 
                    MovieId = s.MovieId,
                    MovieTitle = s.Movie.Title, 
                    RoomId = s.RoomId,
                    RoomName = s.Room.Name, 
                    s.StartTime, 
                    s.Price 
                })
                .ToListAsync();

            var moviesContext = string.Join(", ", movies.Select(m => $"{m.Title} (ID: {m.Id})"));
            var roomsContext = string.Join(", ", rooms.Select(r => $"{r.Name} (ID: {r.Id})"));
            var sessionsContext = string.Join("\n", sessions.Select(s => $"- ID: {s.Id} | MovieID: {s.MovieId} | RoomID: {s.RoomId} | {s.MovieTitle} | {s.RoomName} | {s.StartTime:yyyy-MM-dd HH:mm} | {s.Price}€"));

            var systemPrompt = $@"Tu és um assistente de gestão de cinema. Podes CRIAR, EDITAR ou ELIMINAR sessões.

FILMES DISPONÍVEIS: {moviesContext}
SALAS DISPONÍVEIS: {roomsContext}

SESSÕES ATUAIS (Futuras):
{sessionsContext}

O utilizador pode pedir para criar novas sessões, alterar existentes ou apagar.
Responde APENAS em JSON válido no formato:
{{
  ""sessions"": [
    {{
      ""action"": ""CREATE"" | ""EDIT"" | ""DELETE"",
      ""id"": 123 (para EDIT/DELETE),
      ""movieTitle"": ""Título do Filme"",
      ""date"": ""2025-12-25"",
      ""time"": ""14:00"",
      ""movieId"": 1 (opcional para DELETE),
      ""roomId"": 1 (opcional para DELETE),
      ""price"": 8.00 (opcional para DELETE)
    }}
  ],
  ""message"": ""Explicação do que vais fazer""
}}

REGRAS:
1. Para EDITAR, identifica a sessão pelo ID na lista acima e preenche os campos com os novos valores.
2. Para ELIMINAR, identifica o ID mas preenche também 'movieTitle', 'date' e 'time' para que o utilizador consiga confirmar o que está a ser apagado.
3. Formatos: Data YYYY-MM-DD, Hora HH:mm.
4. Hoje é {DateTime.Now:yyyy-MM-dd}.
5. Máximo 20 ações por resposta. Responde APENAS o JSON.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\nPedido do utilizador: " + userPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 8192
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            Console.WriteLine($"Calling Gemini API: {url}");
            
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Gemini API Error (HTTP {response.StatusCode}): {responseText}");
                return new GeminiSessionResponse
                {
                    Success = false,
                    Message = $"Erro na API: {response.StatusCode}. Verifica a API key ou o modelo no AI Studio."
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var textContent = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (textContent != null)
                {
                    int firstBrace = textContent.IndexOf('{');
                    int lastBrace = textContent.LastIndexOf('}');
                    if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
                    {
                        textContent = textContent.Substring(firstBrace, lastBrace - firstBrace + 1);
                    }
                }

                var sessionData = JsonSerializer.Deserialize<GeminiSessionData>(textContent ?? "{}", new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new GeminiSessionResponse
                {
                    Success = true,
                    Sessions = sessionData?.Sessions ?? new List<GeneratedSession>(),
                    Message = sessionData?.Message ?? "Sessões geradas com sucesso!"
                };
            }
            catch (Exception ex)
            {
                return new GeminiSessionResponse
                {
                    Success = false,
                    Message = $"Erro ao processar resposta: {ex.Message}"
                };
            }
        }
    }

    public class GeminiSessionResponse
    {
        public bool Success { get; set; }
        public List<GeneratedSession> Sessions { get; set; } = new();
        public string Message { get; set; } = "";
    }

    public class GeminiSessionData
    {
        public List<GeneratedSession> Sessions { get; set; } = new();
        public string Message { get; set; } = "";
    }

    public class GeneratedSession
    {
        public string Action { get; set; } = "CREATE";
        public int? Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public decimal Price { get; set; }
    }
}
