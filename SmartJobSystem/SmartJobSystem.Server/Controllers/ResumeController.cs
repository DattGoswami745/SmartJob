using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobAPI.Helpers;
using System.Text.Json;
using Xceed.Document.NET;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Net.Mime.MediaTypeNames;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;

[ApiController]
[Route("api/resume")]
public class ResumeController : ControllerBase
{
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions(
    [FromServices] IConfiguration config,
    [FromServices] GeminiHelper ai)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Login required" });

        using SqlConnection con =
            new(config.GetConnectionString("DefaultConnection"));
        con.Open();

        var cmd = new SqlCommand(@"
    SELECT 
        u.FullName,
        u.Email,
        p.Skills,
        p.ExperienceYears,
        p.Education,
        p.PreferredLocation
    FROM dbo.Users u
    LEFT JOIN dbo.UserProfiles p ON u.UserId = p.UserId
    WHERE u.UserId = @UserId", con);

        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return NotFound("Profile not found");

        var profile = new ProfileDto
        {
            FullName = reader["FullName"]?.ToString(),
            Email = reader["Email"]?.ToString(),
            Skills = reader["Skills"]?.ToString(),
            Education = reader["Education"]?.ToString(),
            PreferredLocation = reader["PreferredLocation"]?.ToString(),
            ExperienceYears = reader["ExperienceYears"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ExperienceYears"])
        };

        // 🔥 Now AI returns object directly
        var aiResult = await ai.Generate(profile);

        return Ok(new
        {
            fullName = profile.FullName,
            email = profile.Email,
            summary = aiResult.summary,
            skills = aiResult.skills,
            experience = aiResult.experience
        });
    }

    // ================= DOWNLOAD RESUME =================
    [HttpPost("download")]
    public IActionResult DownloadResume([FromBody] ResumeDownloadDto resume)
    {
        if (resume == null)
            return BadRequest("No resume data");

        using var stream = new MemoryStream();

        using (var wordDoc =
            WordprocessingDocument.Create(stream,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            body.Append(CreateHeading(resume.FullName, 32));
            body.Append(CreateParagraph(resume.Email));

            body.Append(CreateHeading("Professional Summary"));
            foreach (var s in resume.Summary)
                body.Append(CreateBullet(s));

            body.Append(CreateHeading("Skills"));
            foreach (var s in resume.Skills)
                body.Append(CreateBullet(s));

            body.Append(CreateHeading("Experience"));
            foreach (var e in resume.Experience)
                body.Append(CreateBullet(e));

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "Resume.docx");
    }

    // ===== WORD HELPERS =====
    private Paragraph CreateHeading(string text, int size = 24)
    {
        return new Paragraph(
            new Run(
                new RunProperties(new Bold(), new FontSize() { Val = size.ToString() }),
                new Text(text ?? "")));
    }

    private Paragraph CreateParagraph(string text)
    {
        return new Paragraph(new Run(new Text(text ?? "")));
    }

    private Paragraph CreateBullet(string text)
    {
        return new Paragraph(new Run(new Text("• " + (text ?? ""))));
    }
}

/* DTO */
public class ProfileDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Skills { get; set; }
    public string Education { get; set; }
    public string PreferredLocation { get; set; }
    public int ExperienceYears { get; set; }
}

public class ResumeDownloadDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<string> Summary { get; set; }
    public List<string> Skills { get; set; }
    public List<string> Experience { get; set; }
}
