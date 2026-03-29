using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Helpers
{
    public class GeminiChatHelper
    {
        private readonly HttpClient _http;
        private readonly DbHelper _db;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiChatHelper> _logger;

        public GeminiChatHelper(HttpClient http, DbHelper db, IConfiguration config, ILogger<GeminiChatHelper> logger)
        {
            _http = http;
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<string> Ask(List<ChatMessage> conversation, string systemContext = "")
        {
            try
            {
                var encryptedKey = await _db.GetParameterValueAsync("Gemini:ChatApiKey") 
                    ?? throw new InvalidOperationException("Gemini:ChatApiKey is not configured in database.");
                
                var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") 
                    ?? throw new InvalidOperationException("SecuritySettings:EncryptionKey is not configured in database.");
                
                var apiKey = SecurityHelper.Decrypt(encryptedKey, encryptionKey);
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
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GEMINI CHAT ERROR ({StatusCode}): {Result}", response.StatusCode, result);
                    return $"AI Chat Error ({response.StatusCode})";
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "GEMINI CHAT PROCESSING FAILED");
                return "AI error occurred. Please try again later.";
            }
        }
    }
}
