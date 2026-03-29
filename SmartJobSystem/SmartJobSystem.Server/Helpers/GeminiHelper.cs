using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using SmartJobAPI.Models;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Helpers
{
    public class GeminiHelper
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly DbHelper _db;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiHelper> _logger;

        public GeminiHelper(DbHelper db, IConfiguration config, ILogger<GeminiHelper> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<AiResumeResult> Generate(ProfileDto profile, List<string> sections)
        {
            try
            {
                var encryptedKey = await _db.GetParameterValueAsync("Gemini:ResumeApiKey") 
                    ?? throw new InvalidOperationException("Gemini:ResumeApiKey is not configured in database.");
                
                var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") 
                    ?? throw new InvalidOperationException("SecuritySettings:EncryptionKey is not configured in database.");
                
                var apiKey = SecurityHelper.Decrypt(encryptedKey, encryptionKey);
                var sectionsList = string.Join(", ", sections);
                var prompt = $@"
You are a professional resume writer. Based on the candidate information provided, DIRECTLY generate professional resume content for the following sections: {sectionsList}.

RULES:
1. DO NOT give advice or suggestions. Write the actual content as it should appear on a resume.
2. Return ONLY VALID JSON.
3. JSON keys MUST exactly match the section titles provided: {sectionsList}.
4. The value for each key must be a LIST of strings (bullet points).
5. Use a professional, action-oriented tone (e.g., 'Developed...', 'Managed...', 'Expert in...').
6. SPECIAL RULE FOR 'Skills': Return skills as ONE or TWO strings containing groups of skills (e.g., 'Core: Java, C#, SQL. Web: HTML, CSS, JS.'), NOT as a list of individual items.

Candidate Info:
Name: {profile.FullName}
Email: {profile.Email}
Profile Skills: {profile.Skills}
Experience: {profile.ExperienceYears} years
Education: {profile.Education}
Location: {profile.PreferredLocation}
";

                var body = new
                {
                    contents = new[]
                    {
                new {
                    parts = new[] { new { text = prompt } }
                }
            }
                };

                var response = await _http.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var result = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("RAW AI RESPONSE: {Result}", result);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GEMINI RESUME ERROR ({StatusCode}): {Result}", response.StatusCode, result);
                    return GetFallback($"AI Service Error");
                }

                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                // ✅ SAFE CHECK
                if (!root.TryGetProperty("candidates", out var candidates))
                {
                    return GetFallback("No candidates returned");
                }

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    return GetFallback("Empty AI response");

                text = text.Replace("```json", "")
                           .Replace("```", "")
                           .Trim();

                var dict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(text);

                if (dict == null)
                    return GetFallback("Invalid JSON from AI");

                return new AiResumeResult { sections = dict };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI RESUME GENERATION FAILED");
                return GetFallback("Internal processing error");
            }
        }

        private AiResumeResult GetFallback(string reason)
        {
            var res = new AiResumeResult();
            res.sections["Summary"] = new List<string> { "Note: " + reason };
            res.sections["Skills"] = new List<string> { "Please complete manually" };
            return res;
        }
    }
}
