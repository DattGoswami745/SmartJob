using System.Text;
using System.Text.Json;
using SmartJobAPI.Models;

namespace SmartJobAPI.Helpers
{
    public class GeminiHelper
    {
        private readonly HttpClient _http = new HttpClient();
        private const string API_KEY = "AIzaSyAcnj0PAUDV0JWvXwlnNJEUJ7qVAB5-Hcw";

        public async Task<AiResumeResult> Generate(ProfileDto profile)
        {
            try
            {
                var prompt = $@"
You are a resume generator AI.

Return ONLY VALID JSON. NO explanation. NO markdown.

JSON FORMAT:
{{
  ""summary"": [""point1"", ""point2""],
  ""skills"": [""skill1"", ""skill2""],
  ""experience"": [""exp1"", ""exp2""]
}}

Candidate:
Name: {profile.FullName}
Email: {profile.Email}
Skills: {profile.Skills}
Experience Years: {profile.ExperienceYears}
Education: {profile.Education}
Preferred Location: {profile.PreferredLocation}
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
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={API_KEY}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("RAW AI RESPONSE:");
                Console.WriteLine(result);

                if (!response.IsSuccessStatusCode)
                {
                    return GetFallback("API HTTP Error");
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

                var parsed = JsonSerializer.Deserialize<AiResumeResult>(text);

                if (parsed == null)
                    return GetFallback("Invalid JSON from AI");

                return parsed;
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
            return new AiResumeResult
            {
                summary = new List<string> { "Summary" },
                skills = new List<string> { "Communication", "Problem Solving" },
                experience = new List<string> { "No experience available" }
            };
        }
    }
}
