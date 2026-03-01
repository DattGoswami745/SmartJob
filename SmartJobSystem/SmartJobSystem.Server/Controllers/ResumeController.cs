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
    [HttpPost("suggestions")]
    public async Task<IActionResult> GetSuggestions(
    [FromBody] List<string> sections,
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

        // 🔥 Pass sections list to AI
        var aiResult = await ai.Generate(profile, sections);

        return Ok(new
        {
            fullName = profile.FullName,
            email = profile.Email,
            suggestions = aiResult.sections
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

            body.Append(CreateNameHeading(resume.FullName));
            body.Append(CreateContactInfo(resume.Email));

            if (resume.Sections != null)
            {
                foreach (var section in resume.Sections)
                {
                    body.Append(CreateSectionTitle(section.Title));
                    if (section.Items != null)
                    {
                        foreach (var item in section.Items)
                        {
                            body.Append(CreateBullet(item));
                        }
                    }
                }
            }

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "Resume.docx");
    }

    // ===== WORD HELPERS =====
    private Paragraph CreateNameHeading(string text)
    {
        var p = new Paragraph(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
        p.Append(new Run(
            new RunProperties(new Bold(), new FontSize() { Val = "48" }, new Color() { Val = "1f3864" }), 
            new Text(text ?? "")));
        return p;
    }

    private Paragraph CreateContactInfo(string text)
    {
        var p = new Paragraph(new ParagraphProperties(
            new Justification() { Val = JustificationValues.Center },
            new SpacingBetweenLines() { After = "400" })); // Add space after header
        p.Append(new Run(
            new RunProperties(new FontSize() { Val = "22" }, new Color() { Val = "595959" }), 
            new Text(text ?? "")));
        return p;
    }

    private Paragraph CreateSectionTitle(string text)
    {
        var pProperties = new ParagraphProperties(
            new SpacingBetweenLines() { Before = "400", After = "100" }, 
            new ParagraphBorders(new BottomBorder() { Val = BorderValues.Single, Size = 12, Space = 1, Color = "1f3864" })
        );
        var p = new Paragraph(pProperties);
        p.Append(new Run(
            new RunProperties(new Bold(), new FontSize() { Val = "28" }, new Caps(), new Color() { Val = "1f3864" }), 
            new Text(text ?? "")));
        return p;
    }

    private Paragraph CreateBullet(string text)
    {
        var pProperties = new ParagraphProperties(
            new Indentation() { Left = "360", Hanging = "360" },
            new SpacingBetweenLines() { Before = "80", After = "80" }
        );
        var p = new Paragraph(pProperties);
        
        var bulletRun = new Run(new Text("•\t")); 
        bulletRun.RunProperties = new RunProperties(new RunFonts() { Ascii = "Symbol" });

        var textRun = new Run(new RunProperties(new FontSize() { Val = "22" }), new Text(text ?? ""));

        p.Append(bulletRun);
        p.Append(textRun);
        return p;
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

public class ResumeSectionDto
{
    public string Title { get; set; }
    public List<string> Items { get; set; }
}

public class ResumeDownloadDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<ResumeSectionDto> Sections { get; set; }
}
