using System.Text;
using System.Text.Json;
using SmartJobAPI.Models;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Helpers
{
    public class GeminiHelper
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey;

        public GeminiHelper(IConfiguration config)
        {
            var encryptedKey = config["Gemini:ResumeApiKey"] ?? throw new InvalidOperationException("Gemini:ResumeApiKey is not configured.");
            var encryptionKey = config["SecuritySettings:EncryptionKey"] ?? throw new InvalidOperationException("SecuritySettings:EncryptionKey is not configured.");
            _apiKey = SecurityHelper.Decrypt(encryptedKey, encryptionKey);
        }

        public async Task<AiResumeResult> Generate(ProfileDto profile, List<string> sections)
        {
            try
            {
                var sectionsList = string.Join(", ", sections);
                var prompt = $@"
You are a resume expert. Generate professional suggestions for the following resume sections: {sectionsList}.

RULES:
1. Return ONLY VALID JSON.
2. JSON keys MUST exactly match the section titles provided: {sectionsList}.
3. The value for each key must be a LIST of strings (bullet points).
4. SPECIAL RULE FOR 'Skills': Return skills as ONE or TWO strings containing groups of skills (e.g., 'Core: Java, C#, SQL. Web: HTML, CSS, JS.'), NOT as a list of individual items.

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
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("RAW AI RESPONSE:");
                Console.WriteLine(result);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GEMINI RESUME ERROR ({response.StatusCode}):");
                    Console.WriteLine(result);
                    return GetFallback($"API Error ({response.StatusCode}): {result}");
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
                Console.WriteLine("AI FAILED:");
                Console.WriteLine(ex.Message);
                return GetFallback(ex.Message);
            }
        }

        private AiResumeResult GetFallback(string reason)
        {
            var res = new AiResumeResult();
            res.sections["Summary"] = new List<string> { "ERROR: " + reason };
            res.sections["Skills"] = new List<string> { "Check Console" };
            return res;
        }
    }
}
