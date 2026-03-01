using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SmartJobAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        /* ================================
           GET PROFILE (SESSION BASED)
        ================================= */
        [HttpGet]
        public IActionResult GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Session expired" });

            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT 
                    u.FullName,
                    u.Email,
                    p.ProfileId,
                    p.Skills,
                    p.ExperienceYears,
                    p.Education,
                    p.PreferredLocation,
                    p.ResumePath
                FROM dbo.Users u
                LEFT JOIN dbo.UserProfiles p ON u.UserId = p.UserId
                WHERE u.UserId = @UserId
            ", con);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return NotFound();

            return Ok(new
            {
                fullName = reader["FullName"],
                email = reader["Email"],
                skills = reader["Skills"]?.ToString() ?? "",
                experienceYears = reader["ExperienceYears"] == DBNull.Value ? 0 : (int)reader["ExperienceYears"],
                education = reader["Education"]?.ToString() ?? "",
                preferredLocation = reader["PreferredLocation"]?.ToString() ?? "",
                resumePath = reader["ResumePath"]?.ToString() ?? ""
            });
        }

        /* ================================
           UPDATE PROFILE
        ================================= */
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if profile exists
            var check = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.UserProfiles WHERE UserId=@UserId", con);
            check.Parameters.AddWithValue("@UserId", userId);

            int exists = (int)check.ExecuteScalar();

            SqlCommand cmd;

            if (exists == 0)
            {
                // INSERT
                cmd = new SqlCommand(@"
                    INSERT INTO dbo.UserProfiles
                    (UserId, Skills, ExperienceYears, Education, PreferredLocation, ResumePath)
                    VALUES
                    (@UserId, @Skills, @ExperienceYears, @Education, @PreferredLocation, @ResumePath)
                ", con);
            }
            else
            {
                // UPDATE
                cmd = new SqlCommand(@"
                    UPDATE dbo.UserProfiles SET
                        Skills=@Skills,
                        ExperienceYears=@ExperienceYears,
                        Education=@Education,
                        PreferredLocation=@PreferredLocation,
                        ResumePath=@ResumePath
                    WHERE UserId=@UserId
                ", con);
            }

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Skills", dto.Skills ?? "");
            cmd.Parameters.AddWithValue("@ExperienceYears", dto.ExperienceYears);
            cmd.Parameters.AddWithValue("@Education", dto.Education ?? "");
            cmd.Parameters.AddWithValue("@PreferredLocation", dto.PreferredLocation ?? "");
            cmd.Parameters.AddWithValue("@ResumePath", dto.ResumePath ?? "");

            cmd.ExecuteNonQuery();

            return Ok(new { message = "Profile updated successfully" });
        }

        /* ================================
           UPLOAD RESUME
        ================================= */
        [HttpPost("upload-resume")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Session expired" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file selected." });

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
                return BadRequest(new { message = "Invalid file type. Only PDF and Word documents are allowed." });

            // Ensure directory exists
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "resumes");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // The URL path to save in DB
            var dbPath = $"/uploads/resumes/{uniqueFileName}";

            // Update DB
            using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if profile exists
            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.UserProfiles WHERE UserId=@UserId", con);
            checkCmd.Parameters.AddWithValue("@UserId", userId);
            int exists = (int)checkCmd.ExecuteScalar();

            SqlCommand cmd;
            if (exists == 0)
            {
                // INSERT
                cmd = new SqlCommand(@"
                    INSERT INTO dbo.UserProfiles (UserId, ResumePath)
                    VALUES (@UserId, @ResumePath)", con);
            }
            else
            {
                // UPDATE
                cmd = new SqlCommand(@"
                    UPDATE dbo.UserProfiles SET ResumePath=@ResumePath
                    WHERE UserId=@UserId", con);
            }

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ResumePath", dbPath);
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Resume uploaded successfully.", resumePath = dbPath });
        }
    }

    /* ================================
       DTO
    ================================= */
    public class UserProfileDto
    {
        public string Skills { get; set; }
        public int ExperienceYears { get; set; }
        public string Education { get; set; }
        public string PreferredLocation { get; set; }
        public string ResumePath { get; set; }
    }
}
