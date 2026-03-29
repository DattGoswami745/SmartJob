using Microsoft.AspNetCore.Mvc;
using SmartJobSystem.Server.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartJobSystem.Server.Helpers;

namespace SmartJobSystem.Server.Controllers
{
    [Route("api/central/applications")]
    [ApiController]
    public class CentralApplicationsController : ControllerBase
    {
        private readonly DbHelper _db;
        private readonly IConfiguration _config;

        public CentralApplicationsController(DbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            var apps = await _db.GetAllApplicationsAsync();
            return Ok(apps);
        }

        [HttpDelete("{appId}")]
        public async Task<IActionResult> DeleteApplication(int appId)
        {
            bool deleted = await _db.DeleteApplicationAsync(appId);
            if (!deleted)
                return NotFound(new { message = "Application not found or already deleted." });

            return Ok(new { message = "Application deleted successfully." });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfileForAdmin(int userId)
        {
            var profile = await _db.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey") ?? "";
            var profileData = profile as dynamic;
            string resumePath = profileData?.ResumePath ?? "";

            // Decrypt ResumePath if it looks encrypted
            if (!string.IsNullOrEmpty(resumePath) && !resumePath.StartsWith("/api/"))
            {
                try {
                    resumePath = SecurityHelper.Decrypt(resumePath, encryptionKey);
                } catch { /* Skip if not encrypted */ }
            }

            return Ok(new {
                profileId = profileData.ProfileId,
                userId = profileData.UserId,
                skills = profileData.Skills,
                experienceYears = profileData.ExperienceYears,
                education = profileData.Education,
                preferredLocation = profileData.PreferredLocation,
                resumePath = resumePath,
                resumeFileName = profileData.ResumeFileName
            });
        }
    }
}
