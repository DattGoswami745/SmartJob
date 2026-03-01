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
        [FromServices] IConfiguration config)
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

        // IMPORTANT: Update your GeminiChatHelper to accept conversation
        var aiResponse = await ai.Ask(conversation);

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
