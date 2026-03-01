using System.Text;
using System.Text.Json;

namespace SmartJobAPI.Helpers
{
    public class GeminiChatHelper
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiChatHelper(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
        }

        public async Task<string> Ask(List<ChatMessage> conversation)
        {
            try
            {
                var contents = conversation.Select(m => new
                {
                    role = m.Role,
                    parts = new[] { new { text = m.Content } }
                }).ToList();

                var body = new
                {
                    contents = contents
                };

                var response = await _http.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "Gemini API error.";

                using var doc = JsonDocument.Parse(result);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Empty response.";
            }
            catch
            {
                return "AI error occurred.";
            }
        }
    }
}
