using System.Text;
using System.Text.Json;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Helpers
{
    public class GeminiChatHelper
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiChatHelper(HttpClient http, IConfiguration config)
        {
            _http = http;
            var encryptedKey = config["Gemini:ChatApiKey"] ?? throw new InvalidOperationException("Gemini:ChatApiKey is not configured.");
            var encryptionKey = config["SecuritySettings:EncryptionKey"] ?? throw new InvalidOperationException("SecuritySettings:EncryptionKey is not configured.");
            _apiKey = SecurityHelper.Decrypt(encryptedKey, encryptionKey);
        }

        public async Task<string> Ask(List<ChatMessage> conversation, string systemContext = "")
        {
            try
            {
                var contents = new List<object>();

                if (!string.IsNullOrEmpty(systemContext))
                {
                    contents.Add(new
                    {
                        role = "user",
                        parts = new[] { new { text = "SYSTEM INSTRUCTIONS: " + systemContext } }
                    });
                    contents.Add(new
                    {
                        role = "model",
                        parts = new[] { new { text = "Understood. I will act as a personal career assistant with this context." } }
                    });
                }

                foreach (var m in conversation)
                {
                    contents.Add(new
                    {
                        role = m.Role,
                        parts = new[] { new { text = m.Content } }
                    });
                }

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
                {
                    Console.WriteLine($"GEMINI CHAT ERROR ({response.StatusCode}):");
                    Console.WriteLine(result);
                    return $"AI API Error ({response.StatusCode}): {result}";
                }

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
