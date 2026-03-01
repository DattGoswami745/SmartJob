using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobAPI.Helpers;
using System.Text.Json;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ask(
        [FromBody] ChatRequest request,
        [FromServices] IConfiguration config,
        [FromServices] GeminiChatHelper ai)
    {
        // ✅ Check session
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Login required" });

        if (request == null || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message required");

        // ✅ Get user name from DB
        string fullName = "";

        using SqlConnection con =
            new(config.GetConnectionString("DefaultConnection"));
        con.Open();

        var cmd = new SqlCommand(
            "SELECT FullName FROM dbo.Users WHERE UserId = @UserId",
            con);

        cmd.Parameters.AddWithValue("@UserId", userId);

        var result = cmd.ExecuteScalar();
        if (result != null)
            fullName = result.ToString();

        // ===============================
        // 🔥 CHAT MEMORY SECTION
        // ===============================

        var sessionData = HttpContext.Session.GetString("ChatHistory");

        List<ChatMessage> conversation =
            string.IsNullOrEmpty(sessionData)
            ? new List<ChatMessage>()
            : JsonSerializer.Deserialize<List<ChatMessage>>(sessionData);

        // First time → add personalization
        if (!conversation.Any())
        {
            conversation.Add(new ChatMessage
            {
                Role = "user",
                Content = $"The user's name is {fullName}. Address them by name."
            });
        }

        // Add current user message
        conversation.Add(new ChatMessage
        {
            Role = "user",
            Content = request.Message
        });

        // ✅ Fetch Context Data
        var jobsList = new List<string>();
        var userProfile = "";

        using (var conContext = new SqlConnection(config.GetConnectionString("DefaultConnection")))
        {
            await conContext.OpenAsync();
            
            // Fetch Jobs
            var jobCmd = new SqlCommand("SELECT TOP 10 Title, CompanyName, Location FROM Jobs j JOIN Companies c ON j.CompanyId = c.CompanyId WHERE j.IsActive = 1", conContext);
            using (var reader = await jobCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    jobsList.Add($"{reader["Title"]} at {reader["CompanyName"]} ({reader["Location"]})");
                }
            }

            // Fetch User Profile
            var profileCmd = new SqlCommand("SELECT Skills, ExperienceYears, Education FROM UserProfiles WHERE UserId = @UserId", conContext);
            profileCmd.Parameters.AddWithValue("@UserId", userId);
            using (var reader = await profileCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    userProfile = $"Skills: {reader["Skills"]}, Experience: {reader["ExperienceYears"]} years, Education: {reader["Education"]}";
                }
            }
        }

        var systemContext = $@"
You are the personal career assistant for the Smart Job Management System.
Current User: {fullName}. Profile: {userProfile}.
Available Jobs in our system: {string.Join(" | ", jobsList)}.

Your Goal: Help the user with their career, suggest jobs from the list above if they match their skills, and be encouraging. Keep answers simple and helpful.
";

        // IMPORTANT: Update your GeminiChatHelper to accept conversation
        var aiResponse = await ai.Ask(conversation, systemContext);

        // Add AI response
        conversation.Add(new ChatMessage
        {
            Role = "model",
            Content = aiResponse
        });

        // Save back to session
        HttpContext.Session.SetString(
            "ChatHistory",
            JsonSerializer.Serialize(conversation)
        );

        return Ok(aiResponse);
    }

    // Optional: Clear chat endpoint
    [HttpPost("clear")]
    public IActionResult ClearChat()
    {
        HttpContext.Session.Remove("ChatHistory");
        return Ok("Chat cleared");
    }
}

public class ChatRequest
{
    public string Message { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } // "user" or "model"
    public string Content { get; set; }
}
