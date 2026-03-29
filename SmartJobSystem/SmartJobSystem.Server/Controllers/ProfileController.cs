using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SmartJobSystem.Server.Data;
using SmartJobSystem.Server.Helpers;

namespace SmartJobAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly DbHelper _db;

        public ProfileController(IConfiguration config, IWebHostEnvironment env, DbHelper db)
        {
            _config = config;
            _env = env;
            _db = db;
        }

        /* ================================
           GET PROFILE (SESSION BASED)
        ================================= */
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Session expired" });

            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT 
                    u.FullName,
                    u.Email,
                    p.ProfileId,
                    p.Skills,
                    p.ExperienceYears,
                    p.Education,
                    p.PreferredLocation,
                    p.ResumePath,
                    p.ResumeFileName
                FROM dbo.Users u
                LEFT JOIN dbo.UserProfiles p ON u.UserId = p.UserId
                WHERE u.UserId = @UserId
            ", con);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return NotFound();

            string resumePath = reader["ResumePath"]?.ToString() ?? "";
            string resumeFileName = reader["ResumeFileName"]?.ToString() ?? "";
            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") ?? "";

            // Decrypt ResumePath if it exists and looks encrypted
            if (!string.IsNullOrEmpty(resumePath) && !resumePath.StartsWith("/api/"))
            {
                try {
                    resumePath = SecurityHelper.Decrypt(resumePath, encryptionKey);
                } catch { /* Not encrypted or already plain */ }
            }

            return Ok(new
            {
                fullName = reader["FullName"],
                email = reader["Email"],
                skills = reader["Skills"]?.ToString() ?? "",
                experienceYears = reader["ExperienceYears"] == DBNull.Value ? 0 : (int)reader["ExperienceYears"],
                education = reader["Education"]?.ToString() ?? "",
                preferredLocation = reader["PreferredLocation"]?.ToString() ?? "",
                resumePath = resumePath,
                resumeFileName = resumeFileName
            });
        }

        /* ================================
           UPDATE PROFILE
        ================================= */
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            using SqlConnection con =
                new(_config.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();

            // Check if profile exists
            var check = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.UserProfiles WHERE UserId=@UserId", con);
            check.Parameters.AddWithValue("@UserId", userId);

            int exists = (int)await check.ExecuteScalarAsync();

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
            string resumePathToSave = dto.ResumePath ?? "";
            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") ?? "";

            // Always encrypt before saving back to DB
            if (!string.IsNullOrEmpty(resumePathToSave))
            {
                try {
                    resumePathToSave = SecurityHelper.Encrypt(resumePathToSave, encryptionKey);
                } catch { /* Handle error or keep as is */ }
            }

            cmd.Parameters.AddWithValue("@ResumePath", resumePathToSave);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        /* ================================
           UPLOAD RESUME (STORE IN DB)
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

            // Read file content as byte array
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") ?? "";

            // ENCRYPT THE FILE BEFORE SAVING
            byte[] encryptedFileBytes = SecurityHelper.EncryptBytes(fileBytes, encryptionKey);

            // Save encrypted binary data to DB via DbHelper
            bool success = await _db.UpdateUserResumeBinaryAsync(userId.Value, encryptedFileBytes, file.FileName, file.ContentType);

            if (!success)
                return StatusCode(500, new { message = "Failed to save resume to database." });

            // Backward compatibility: update ResumePath to point to the new download endpoint
            var downloadPathPlain = $"/api/profile/download-resume/{userId}";
            
            // ENCRYPT THE PATH string
            var encryptedDownloadPath = SecurityHelper.Encrypt(downloadPathPlain, encryptionKey);
            
            using SqlConnection con = new(_config.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            var updatePathCmd = new SqlCommand("UPDATE dbo.UserProfiles SET ResumePath=@ResumePath WHERE UserId=@UserId", con);
            updatePathCmd.Parameters.AddWithValue("@UserId", userId);
            updatePathCmd.Parameters.AddWithValue("@ResumePath", encryptedDownloadPath);
            await updatePathCmd.ExecuteNonQueryAsync();

            return Ok(new { message = "Resume uploaded successfully (Encrypted).", resumePath = downloadPathPlain });
        }

        /* ================================
           DOWNLOAD RESUME (FETCH FROM DB)
        ================================= */
        [HttpGet("download-resume/{userId?}")]
        public async Task<IActionResult> DownloadResume(int? userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized(new { message = "Session expired" });

            // If userId is provided, ensure current user has permission (self, admin, or recruiter)
            int targetUserId = userId ?? currentUserId.Value;

            // Basic permission check
            if (targetUserId != currentUserId.Value)
            {
                var role = HttpContext.Session.GetString("Role");
                if (role != "SuperAdmin" && role != "Company" && role != "Central")
                {
                    return Unauthorized(new { message = "You do not have permission to view this resume." });
                }
            }

            var resume = await _db.GetUserResumeBinaryAsync(targetUserId);

            if (resume == null || resume.FileContent == null)
                return NotFound(new { message = "Resume not found in database." });

            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") ?? "";

            // DECRYPT THE FILE CONTENT
            byte[] decryptedFileBytes;
            try {
                decryptedFileBytes = SecurityHelper.DecryptBytes((byte[])resume.FileContent, encryptionKey);
            } catch {
                decryptedFileBytes = (byte[])resume.FileContent; // Fallback if not encrypted
            }

            // ENSURE THE FILE IS VIEWABLE INLINE
            string fileName = (string)resume.FileName ?? "Resume.pdf";
            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");

            return File(
                decryptedFileBytes, 
                (string)resume.ContentType ?? "application/octet-stream"
            );
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
